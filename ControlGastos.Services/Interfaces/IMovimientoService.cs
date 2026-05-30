using ControlGastos.Data.Entities;

namespace ControlGastos.Services.Interfaces;

public interface IMovimientoService
{
    Task<IEnumerable<Movimiento>> ObtenerPorPeriodoAsync(int mes, int anio, int? idCuenta = null);
    Task<IEnumerable<Movimiento>> ObtenerPorRangoAsync(DateOnly desde, DateOnly hasta);
    Task<Movimiento?> ObtenerPorIdAsync(int id);

    // Listas para dropdowns
    Task<IEnumerable<Cuenta>> ObtenerCuentasAsync();
    Task<IEnumerable<Categoria>> ObtenerCategoriasAsync();
    Task<IEnumerable<SubCategoria>> ObtenerSubCategoriasPorCategoriaAsync(int idCategoria);
    Task<IEnumerable<TiposMovimiento>> ObtenerTiposMovimientoAsync();
    Task<IEnumerable<FormasDePago>> ObtenerFormasDePagoAsync();

    // CRUD genérico
    Task CreateAsync(Movimiento movimiento);
    Task UpdateAsync(Movimiento movimiento);
    Task DeleteAsync(int id);

    // Lógica específica de pasajes entre cuentas
    Task<int> ObtenerIdCategoriaPasajeAsync();
    Task CreatePasajeAsync(Movimiento origen, int idCuentaDestino);
    Task UpdatePasajeAsync(Movimiento movimiento, int idCuentaDestino);
}
