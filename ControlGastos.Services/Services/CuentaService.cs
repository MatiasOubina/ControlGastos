using ControlGastos.Data.Context;
using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Services.Services;

public class CuentaService : ICuentaService
{
    private readonly ControlGastosDbContext _context;

    public CuentaService(ControlGastosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Cuenta>> ObtenerTodasLasCuentas()
        => await _context.Cuentas.OrderBy(c => c.Descripcion).ToListAsync();

    public async Task<Cuenta?> ObtenerCuentaPorIdAsync(int id)
        => await _context.Cuentas.FindAsync(id);

    public async Task<bool> ExisteDescripcionAsync(string descripcion, int? excludeId = null)
        => await _context.Cuentas.AnyAsync(c =>
            c.Descripcion == descripcion && (excludeId == null || c.Id != excludeId));

    public async Task CreateAsync(Cuenta cuenta)
    {
        _context.Cuentas.Add(cuenta);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Cuenta cuenta)
    {
        _context.Cuentas.Update(cuenta);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var cuenta = await _context.Cuentas.FindAsync(id);
        if (cuenta != null)
        {
            _context.Cuentas.Remove(cuenta);
            await _context.SaveChangesAsync();
        }
    }
}
