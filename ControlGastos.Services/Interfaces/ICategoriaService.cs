using ControlGastos.Data.Entities;

namespace ControlGastos.Services.Interfaces;

public interface ICategoriaService
{
    Task<IEnumerable<Categoria>> ObtenerTodasLasCategorias();
    Task<Categoria?> ObtenerCategoriaPorIdAsync(int id);
    Task<bool> ExisteDescripcionAsync(string descripcion, int? excludeId = null);
    Task CreateAsync(Categoria categoria);
    Task UpdateAsync(Categoria categoria);
    Task DeleteAsync(int id);
}
