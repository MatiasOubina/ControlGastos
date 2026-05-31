using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Web.Models;

public class VerificarTotpViewModel
{
    [Required(ErrorMessage = "Ingresá el código")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "El código tiene 6 dígitos")]
    [Display(Name = "Código de Google Authenticator")]
    public string Codigo { get; set; } = string.Empty;

    public int UserId { get; set; }
    public string? ReturnUrl { get; set; }
}
