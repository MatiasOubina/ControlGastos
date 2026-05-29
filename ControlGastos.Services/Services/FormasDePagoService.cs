using ControlGastos.Data.Context;
using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Services.Services;

public class FormasDePagoService : IFormasDePagoService
{
    private readonly ControlGastosDbContext _context;

    public FormasDePagoService(ControlGastosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FormasDePago>> ObtenerTodasLasFormasDePago()
        => await _context.FormasDePagos.OrderBy(f => f.Descripcion).ToListAsync();

    public async Task<FormasDePago?> ObtenerFormaDePagoPorIdAsync(int id)
        => await _context.FormasDePagos.FindAsync(id);

    public async Task<bool> ExisteDescripcionAsync(string descripcion, int? excludeId = null)
        => await _context.FormasDePagos.AnyAsync(f =>
            f.Descripcion == descripcion && (excludeId == null || f.Id != excludeId));

    public async Task CreateAsync(FormasDePago formaDePago)
    {
        _context.FormasDePagos.Add(formaDePago);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(FormasDePago formaDePago)
    {
        _context.FormasDePagos.Update(formaDePago);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var formaDePago = await _context.FormasDePagos.FindAsync(id);
        if (formaDePago != null)
        {
            _context.FormasDePagos.Remove(formaDePago);
            await _context.SaveChangesAsync();
        }
    }
}
