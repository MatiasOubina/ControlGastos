using ControlGastos.Data.Entities;

namespace ControlGastos.Services.Interfaces;

public interface ITiposMovimientoService
{
    Task<IEnumerable<TiposMovimiento>> ObtenerTodosLosTiposMovimiento();
    Task<TiposMovimiento?> ObtenerTipoMovimientoPorIdAsync(int id);
    Task<bool> ExisteDescripcionAsync(string descripcion, int? excludeId = null);
    Task CreateAsync(TiposMovimiento tipoMovimiento);
    Task UpdateAsync(TiposMovimiento tipoMovimiento);
    Task DeleteAsync(int id);
}
