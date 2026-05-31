using System.Security.Claims;
using ControlGastos.Services.Interfaces;
using ControlGastos.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace ControlGastos.Web.Controllers;

public class AccountController(IAuthService authService) : Controller
{
    // ── Login (paso 1: credenciales / paso 2: TOTP — misma pantalla) ──────────

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.NombreUsuario))
        {
            ModelState.AddModelError(nameof(model.NombreUsuario), "Ingresá el nombre de usuario.");
            return View(model);
        }

        // ── Opción A: login con Google Authenticator directo ──────────────────
        if (model.Metodo == "totp")
        {
            if (string.IsNullOrWhiteSpace(model.Codigo))
            {
                ModelState.AddModelError(nameof(model.Codigo), "Ingresá el código de 6 dígitos.");
                return View(model);
            }

            var userId = await authService.LoginConTotpAsync(model.NombreUsuario, model.Codigo!);
            if (userId is null)
            {
                ModelState.AddModelError(nameof(model.Codigo), "Código incorrecto, expirado, o el usuario no tiene Authenticator habilitado.");
                return View(model);
            }

            await EmitirJwtCookieAsync(userId.Value);
            return Redireccionar(model.ReturnUrl);
        }

        // ── Opción B paso 2: credenciales ya validadas, verificar código TOTP ─
        if (model.RequiereTotp && model.PreAuthUserId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(model.Codigo))
            {
                ModelState.AddModelError(nameof(model.Codigo), "Ingresá el código de Google Authenticator.");
                return View(model);
            }

            if (!await authService.VerificarTotpAsync(model.PreAuthUserId.Value, model.Codigo!))
            {
                ModelState.AddModelError(nameof(model.Codigo), "Código incorrecto o expirado.");
                return View(model);
            }

            await EmitirJwtCookieAsync(model.PreAuthUserId.Value);
            return Redireccionar(model.ReturnUrl);
        }

        // ── Opción B paso 1: validar contraseña ───────────────────────────────
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Ingresá la contraseña.");
            return View(model);
        }

        var (exitoso, requiereTotp, uid) = await authService.LoginAsync(model.NombreUsuario, model.Password);

        if (!exitoso)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(model);
        }

        if (requiereTotp)
        {
            model.RequiereTotp  = true;
            model.PreAuthUserId = uid;
            model.Password      = string.Empty;
            ModelState.Clear();
            return View(model);
        }

        await EmitirJwtCookieAsync(uid);
        return Redireccionar(model.ReturnUrl);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        return RedirectToAction(nameof(Login));
    }

    // ── Cambiar contraseña ────────────────────────────────────────────────────

    [Authorize]
    [HttpGet]
    public IActionResult CambiarPassword() => View(new CambiarPasswordViewModel());

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarPassword(CambiarPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = ObtenerUserId();
        if (!await authService.CambiarPasswordAsync(userId, model.PasswordActual, model.NuevaPassword))
        {
            ModelState.AddModelError(nameof(model.PasswordActual), "La contraseña actual es incorrecta.");
            return View(model);
        }

        TempData["Success"] = "Contraseña actualizada correctamente.";
        return RedirectToAction("Index", "Home");
    }

    // ── Configurar Google Authenticator ───────────────────────────────────────

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ConfigurarTotp()
    {
        var userId  = ObtenerUserId();
        var usuario = await authService.ObtenerPorIdAsync(userId);

        if (usuario?.TotpHabilitado == true)
            return View(new ConfigurarTotpViewModel { TotpHabilitado = true });

        var secret = await authService.GenerarTotpSecretAsync(userId);
        return View(new ConfigurarTotpViewModel
        {
            Secret         = secret,
            QrCodeDataUri  = GenerarQrDataUri(secret, usuario!.NombreUsuario),
            TotpHabilitado = false
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfigurarTotp(ConfigurarTotpViewModel model)
    {
        var userId = ObtenerUserId();

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Codigo))
        {
            var usuario = await authService.ObtenerPorIdAsync(userId);
            model.QrCodeDataUri = GenerarQrDataUri(model.Secret, usuario!.NombreUsuario);
            return View(model);
        }

        if (!await authService.ConfirmarYHabilitarTotpAsync(userId, model.Codigo!))
        {
            ModelState.AddModelError(nameof(model.Codigo), "Código incorrecto. Intentá de nuevo.");
            var usuario = await authService.ObtenerPorIdAsync(userId);
            model.QrCodeDataUri = GenerarQrDataUri(model.Secret, usuario!.NombreUsuario);
            return View(model);
        }

        TempData["Success"] = "Google Authenticator habilitado correctamente.";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeshabilitarTotp()
    {
        await authService.DeshabilitarTotpAsync(ObtenerUserId());
        TempData["Success"] = "Google Authenticator deshabilitado.";
        return RedirectToAction(nameof(ConfigurarTotp));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task EmitirJwtCookieAsync(int userId)
    {
        var usuario = await authService.ObtenerPorIdAsync(userId);
        var token   = authService.GenerarToken(usuario!);

        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure   = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires  = DateTimeOffset.UtcNow.AddHours(8)
        });
    }

    private int ObtenerUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private IActionResult Redireccionar(string? returnUrl)
        => !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("Index", "Home");

    private string GenerarQrDataUri(string secret, string nombreUsuario)
    {
        var otpauth = $"otpauth://totp/ControlGastos:{Uri.EscapeDataString(nombreUsuario)}" +
                      $"?secret={secret}&issuer=ControlGastos&algorithm=SHA1&digits=6&period=30";

        using var qrGenerator = new QRCodeGenerator();
        using var qrData      = qrGenerator.CreateQrCode(otpauth, QRCodeGenerator.ECCLevel.Q);
        using var qrCode      = new PngByteQRCode(qrData);
        var pngBytes          = qrCode.GetGraphic(5);
        return "data:image/png;base64," + Convert.ToBase64String(pngBytes);
    }
}
