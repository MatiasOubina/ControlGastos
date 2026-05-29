using ControlGastos.Data.Entities;

namespace ControlGastos.Services.Interfaces;

public interface IFormasDePagoService
{
    Task<IEnumerable<FormasDePago>> ObtenerTodasLasFormasDePago();
    Task<FormasDePago?> ObtenerFormaDePagoPorIdAsync(int id);
    Task<bool> ExisteDescripcionAsync(string descripcion, int? excludeId = null);
    Task CreateAsync(FormasDePago formaDePago);
    Task UpdateAsync(FormasDePago formaDePago);
    Task DeleteAsync(int id);
}
