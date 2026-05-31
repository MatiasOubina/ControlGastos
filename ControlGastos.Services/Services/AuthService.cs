using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ControlGastos.Data.Context;
using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OtpNet;

namespace ControlGastos.Services.Services;

public class AuthService(ControlGastosDbContext context, IConfiguration configuration) : IAuthService
{
    // ── Login ─────────────────────────────────────────────────────────────────

    public async Task<(bool exitoso, bool requiereTotp, int userId)> LoginAsync(string nombreUsuario, string password)
    {
        var usuario = await context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Activo);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
            return (false, false, 0);

        return (true, usuario.TotpHabilitado, usuario.Id);
    }

    public async Task<int?> LoginConTotpAsync(string nombreUsuario, string codigo)
    {
        var usuario = await context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Activo && u.TotpHabilitado);

        if (usuario?.TotpSecret is null) return null;
        if (!ValidarCodigo(usuario.TotpSecret, codigo)) return null;

        return usuario.Id;
    }

    // ── JWT ───────────────────────────────────────────────────────────────────

    public string GenerarToken(Usuario usuario)
    {
        var jwtConfig = configuration.GetSection("Jwt");
        var key       = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!));
        var creds     = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name,           usuario.NombreUsuario),
            new Claim("NombreCompleto",          usuario.NombreCompleto),
        };

        var token = new JwtSecurityToken(
            issuer:             jwtConfig["Issuer"],
            audience:           jwtConfig["Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(double.Parse(jwtConfig["ExpiresHours"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ── Contraseña ────────────────────────────────────────────────────────────

    public async Task<bool> CambiarPasswordAsync(int userId, string passwordActual, string nuevaPassword)
    {
        var usuario = await context.Usuarios.FindAsync(userId);
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(passwordActual, usuario.PasswordHash))
            return false;

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetearPasswordAsync(int userId, string nuevaPassword)
    {
        var usuario = await context.Usuarios.FindAsync(userId);
        if (usuario is null) return false;

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
        await context.SaveChangesAsync();
        return true;
    }

    // ── TOTP (Google Authenticator) ───────────────────────────────────────────

    public async Task<string> GenerarTotpSecretAsync(int userId)
    {
        var usuario = await context.Usuarios.FindAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado.");

        // Genera un nuevo secret de 20 bytes (160 bits) — estándar RFC 4226
        var secretBytes = KeyGeneration.GenerateRandomKey(20);
        usuario.TotpSecret   = Base32Encoding.ToString(secretBytes);
        usuario.TotpHabilitado = false; // no habilitado hasta confirmar

        await context.SaveChangesAsync();
        return usuario.TotpSecret;
    }

    public async Task<bool> ConfirmarYHabilitarTotpAsync(int userId, string codigo)
    {
        var usuario = await context.Usuarios.FindAsync(userId);
        if (usuario?.TotpSecret is null) return false;

        if (!ValidarCodigo(usuario.TotpSecret, codigo)) return false;

        usuario.TotpHabilitado = true;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerificarTotpAsync(int userId, string codigo)
    {
        var usuario = await context.Usuarios.FindAsync(userId);
        if (usuario?.TotpSecret is null || !usuario.TotpHabilitado) return false;

        return ValidarCodigo(usuario.TotpSecret, codigo);
    }

    public async Task DeshabilitarTotpAsync(int userId)
    {
        var usuario = await context.Usuarios.FindAsync(userId);
        if (usuario is null) return;

        usuario.TotpSecret    = null;
        usuario.TotpHabilitado = false;
        await context.SaveChangesAsync();
    }

    // ── Admin ─────────────────────────────────────────────────────────────────

    public async Task<List<Usuario>> ObtenerUsuariosAsync()
        => await context.Usuarios.OrderBy(u => u.NombreUsuario).ToListAsync();

    public async Task<Usuario?> ObtenerPorIdAsync(int userId)
        => await context.Usuarios.FindAsync(userId);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool ValidarCodigo(string base32Secret, string codigo)
    {
        var secretBytes = Base32Encoding.ToBytes(base32Secret);
        var totp = new Totp(secretBytes);
        // Permite una ventana de ±1 período (30 seg) para tolerar pequeñas diferencias de reloj
        return totp.VerifyTotp(codigo.Trim(), out _, new VerificationWindow(previous: 1, future: 1));
    }

    // ── Creación inicial de usuario ───────────────────────────────────────────

    public async Task<bool> CrearUsuarioAsync(string nombreUsuario, string password, string nombreCompleto)
    {
        if (await context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario))
            return false;

        context.Usuarios.Add(new Usuario
        {
            NombreUsuario  = nombreUsuario,
            PasswordHash   = BCrypt.Net.BCrypt.HashPassword(password),
            NombreCompleto = nombreCompleto,
            Activo         = true
        });

        await context.SaveChangesAsync();
        return true;
    }
}
