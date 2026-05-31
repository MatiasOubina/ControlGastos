using ControlGastos.Data.Entities;

namespace ControlGastos.Services.Interfaces;

public interface IAuthService
{
    // Login
    Task<(bool exitoso, bool requiereTotp, int userId)> LoginAsync(string nombreUsuario, string password);
    Task<int?> LoginConTotpAsync(string nombreUsuario, string codigo);

    // JWT
    string GenerarToken(Usuario usuario);

    // Contraseña
    Task<bool> CambiarPasswordAsync(int userId, string passwordActual, string nuevaPassword);
    Task<bool> ResetearPasswordAsync(int userId, string nuevaPassword);

    // TOTP
    Task<string> GenerarTotpSecretAsync(int userId);
    Task<bool> ConfirmarYHabilitarTotpAsync(int userId, string codigo);
    Task<bool> VerificarTotpAsync(int userId, string codigo);
    Task DeshabilitarTotpAsync(int userId);

    // Creación
    Task<bool> CrearUsuarioAsync(string nombreUsuario, string password, string nombreCompleto);

    // Admin
    Task<List<Usuario>> ObtenerUsuariosAsync();
    Task<Usuario?> ObtenerPorIdAsync(int userId);
}
