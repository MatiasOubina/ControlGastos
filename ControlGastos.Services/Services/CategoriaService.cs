using ControlGastos.Data.Context;
using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Services.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ControlGastosDbContext _context;

    public CategoriaService(ControlGastosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Categoria>> ObtenerTodasLasCategorias()
        => await _context.Categorias.OrderBy(c => c.Descripcion).ToListAsync();

    public async Task<Categoria?> ObtenerCategoriaPorIdAsync(int id)
        => await _context.Categorias.FindAsync(id);

    public async Task<bool> ExisteDescripcionAsync(string descripcion, int? excludeId = null)
        => await _context.Categorias.AnyAsync(c =>
            c.Descripcion == descripcion && (excludeId == null || c.Id != excludeId));

    public async Task CreateAsync(Categoria categoria)
    {
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Categoria categoria)
    {
        _context.Categorias.Update(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria != null)
        {
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
        }
    }
}
