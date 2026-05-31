using System.ComponentModel.DataAnnotations;

namespace ControlGastos.Web.Models;

public class CambiarPasswordViewModel
{
    [Required(ErrorMessage = "Requerido")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string PasswordActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Requerido")]
    [MinLength(8, ErrorMessage = "Mínimo 8 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string NuevaPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Requerido")]
    [DataType(DataType.Password)]
    [Compare(nameof(NuevaPassword), ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar nueva contraseña")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}
