using ControlGastos.Data.Entities;

namespace ControlGastos.Services.Interfaces;

public interface ICuentaService
{
    Task<IEnumerable<Cuenta>> ObtenerTodasLasCuentas();
    Task<Cuenta?> ObtenerCuentaPorIdAsync(int id);
    Task<bool> ExisteDescripcionAsync(string descripcion, int? excludeId = null);
    Task CreateAsync(Cuenta cuenta);
    Task UpdateAsync(Cuenta cuenta);
    Task DeleteAsync(int id);
}
