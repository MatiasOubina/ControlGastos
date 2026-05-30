using ControlGastos.Data.Entities;

namespace ControlGastos.Services.Interfaces;

public interface ISaldosInicialesService
{
    Task<IEnumerable<SaldosIniciale>> ObtenerTodosAsync();
    Task<IEnumerable<SaldosIniciale>> ObtenerPorPeriodoAsync(int mes, int anio);
    Task<SaldosIniciale?> ObtenerPorIdAsync(int id);
    Task<bool> ExistePeriodoAsync(int idCuenta, byte mes, short anio, int? excludeId = null);
    Task<IEnumerable<Cuenta>> ObtenerCuentasAsync();
    Task CreateAsync(SaldosIniciale saldo);
    Task UpdateAsync(SaldosIniciale saldo);
    Task DeleteAsync(int id);
}
