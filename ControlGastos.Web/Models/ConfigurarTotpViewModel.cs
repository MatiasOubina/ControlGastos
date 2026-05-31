using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Web.Models;

public class ConfigurarTotpViewModel
{
    public string Secret { get; set; } = string.Empty;
    public string QrCodeDataUri { get; set; } = string.Empty;
    public bool TotpHabilitado { get; set; }

    [StringLength(6, MinimumLength = 6, ErrorMessage = "El código tiene 6 dígitos")]
    [Display(Name = "Código de verificación")]
    public string? Codigo { get; set; }
}
