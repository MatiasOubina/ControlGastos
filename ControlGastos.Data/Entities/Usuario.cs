namespace ControlGastos.Data.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public bool Activo { get; set; } = true;
    public string? TotpSecret { get; set; }
    public bool TotpHabilitado { get; set; } = false;
}
