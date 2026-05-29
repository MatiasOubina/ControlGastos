using ControlGastos.Data.Context;
using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Services.Services;

public class TiposMovimientoService : ITiposMovimientoService
{
    private readonly ControlGastosDbContext _context;

    public TiposMovimientoService(ControlGastosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TiposMovimiento>> ObtenerTodosLosTiposMovimiento()
        => await _context.TiposMovimientos.OrderBy(t => t.Descripcion).ToListAsync();

    public async Task<TiposMovimiento?> ObtenerTipoMovimientoPorIdAsync(int id)
        => await _context.TiposMovimientos.FindAsync(id);

    public async Task<bool> ExisteDescripcionAsync(string descripcion, int? excludeId = null)
        => await _context.TiposMovimientos.AnyAsync(t =>
            t.Descripcion == descripcion && (excludeId == null || t.Id != excludeId));

    public async Task CreateAsync(TiposMovimiento tipoMovimiento)
    {
        _context.TiposMovimientos.Add(tipoMovimiento);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TiposMovimiento tipoMovimiento)
    {
        _context.TiposMovimientos.Update(tipoMovimiento);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var tipo = await _context.TiposMovimientos.FindAsync(id);
        if (tipo != null)
        {
            _context.TiposMovimientos.Remove(tipo);
            await _context.SaveChangesAsync();
        }
    }
}
