using ControlGastos.Data.Context;
using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Services.Services;

public class SubCategoriaService : ISubCategoriaService
{
    private readonly ControlGastosDbContext _context;

    public SubCategoriaService(ControlGastosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SubCategoria>> ObtenerTodasLasSubCategorias()
        => await _context.SubCategorias
            .Include(sc => sc.IdCategoriaNavigation)
            .OrderBy(sc => sc.IdCategoriaNavigation.Descripcion)
            .ThenBy(sc => sc.Descripcion)
            .ToListAsync();

    public async Task<SubCategoria?> ObtenerSubCategoriaPorIdAsync(int id)
        => await _context.SubCategorias
            .Include(sc => sc.IdCategoriaNavigation)
            .FirstOrDefaultAsync(sc => sc.Id == id);

    // La restricción única es (Descripcion + IdCategoria), no solo Descripcion
    public async Task<bool> ExisteDescripcionAsync(string descripcion, int idCategoria, int? excludeId = null)
        => await _context.SubCategorias.AnyAsync(sc =>
            sc.Descripcion == descripcion &&
            sc.IdCategoria == idCategoria &&
            (excludeId == null || sc.Id != excludeId));

    public async Task<IEnumerable<Categoria>> ObtenerCategoriasAsync()
        => await _context.Categorias.OrderBy(c => c.Descripcion).ToListAsync();

    public async Task CreateAsync(SubCategoria subCategoria)
    {
        _context.SubCategorias.Add(subCategoria);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SubCategoria subCategoria)
    {
        _context.SubCategorias.Update(subCategoria);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var subCategoria = await _context.SubCategorias.FindAsync(id);
        if (subCategoria != null)
        {
            _context.SubCategorias.Remove(subCategoria);
            await _context.SaveChangesAsync();
        }
    }
}
