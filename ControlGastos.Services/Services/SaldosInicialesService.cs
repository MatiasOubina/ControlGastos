using ControlGastos.Data.Context;
using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Services.Services;

public class SaldosInicialesService : ISaldosInicialesService
{
    private readonly ControlGastosDbContext _context;

    public SaldosInicialesService(ControlGastosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SaldosIniciale>> ObtenerTodosAsync()
        => await _context.SaldosIniciales
            .Include(s => s.IdCuentaNavigation)
            .OrderByDescending(s => s.Anio)
            .ThenByDescending(s => s.Mes)
            .ThenBy(s => s.IdCuentaNavigation.Descripcion)
            .ToListAsync();

    public async Task<IEnumerable<SaldosIniciale>> ObtenerPorPeriodoAsync(int mes, int anio)
        => await _context.SaldosIniciales
            .Include(s => s.IdCuentaNavigation)
            .Where(s => s.Mes == mes && s.Anio == anio)
            .OrderBy(s => s.IdCuentaNavigation.Descripcion)
            .ToListAsync();

    public async Task<SaldosIniciale?> ObtenerPorIdAsync(int id)
        => await _context.SaldosIniciales
            .Include(s => s.IdCuentaNavigation)
            .FirstOrDefaultAsync(s => s.Id == id);

    // Restricción única: (IdCuenta + Mes + Anio)
    public async Task<bool> ExistePeriodoAsync(int idCuenta, byte mes, short anio, int? excludeId = null)
        => await _context.SaldosIniciales.AnyAsync(s =>
            s.IdCuenta == idCuenta &&
            s.Mes == mes &&
            s.Anio == anio &&
            (excludeId == null || s.Id != excludeId));

    public async Task<IEnumerable<Cuenta>> ObtenerCuentasAsync()
        => await _context.Cuentas.OrderBy(c => c.Descripcion).ToListAsync();

    public async Task CreateAsync(SaldosIniciale saldo)
    {
        _context.SaldosIniciales.Add(saldo);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SaldosIniciale saldo)
    {
        _context.SaldosIniciales.Update(saldo);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var saldo = await _context.SaldosIniciales.FindAsync(id);
        if (saldo != null)
        {
            _context.SaldosIniciales.Remove(saldo);
            await _context.SaveChangesAsync();
        }
    }
}
