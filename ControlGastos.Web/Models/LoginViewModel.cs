using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Web.Models;

public class LoginViewModel
{
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [StringLength(6, MinimumLength = 6, ErrorMessage = "El código tiene 6 dígitos")]
    [Display(Name = "Código de Google Authenticator")]
    public string? Codigo { get; set; }

    // "password" | "totp"
    public string Metodo { get; set; } = "password";

    // Estado intermedio: credenciales OK, esperando código TOTP
    public bool RequiereTotp { get; set; }
    public int? PreAuthUserId { get; set; }

    public string? ReturnUrl { get; set; }
}
