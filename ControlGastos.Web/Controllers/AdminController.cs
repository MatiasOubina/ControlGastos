using System.ComponentModel.DataAnnotations;
using ControlGastos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControlGastos.Web.Controllers;

[Authorize]
public class AdminController(IAuthService authService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Usuarios()
    {
        var usuarios = await authService.ObtenerUsuariosAsync();
        return View(usuarios);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetearPassword(int userId, [Required][MinLength(8)] string nuevaPassword)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "La contraseña debe tener al menos 8 caracteres.";
            return RedirectToAction(nameof(Usuarios));
        }

        var ok = await authService.ResetearPasswordAsync(userId, nuevaPassword);
        TempData[ok ? "Success" : "Error"] = ok
            ? "Contraseña reseteada correctamente."
            : "Usuario no encontrado.";

        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeshabilitarTotp(int userId)
    {
        await authService.DeshabilitarTotpAsync(userId);
        TempData["Success"] = "TOTP deshabilitado para el usuario.";
        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearUsuario(string nombreUsuario, string password, string nombreCompleto)
    {
        var ok = await authService.CrearUsuarioAsync(nombreUsuario, password, nombreCompleto);
        TempData[ok ? "Success" : "Error"] = ok
            ? $"Usuario '{nombreUsuario}' creado."
            : $"Ya existe un usuario con ese nombre.";

        return RedirectToAction(nameof(Usuarios));
    }
}
