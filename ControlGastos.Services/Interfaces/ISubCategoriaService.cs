using ControlGastos.Data.Entities;

namespace ControlGastos.Services.Interfaces;

public interface ISubCategoriaService
{
    Task<IEnumerable<SubCategoria>> ObtenerTodasLasSubCategorias();
    Task<SubCategoria?> ObtenerSubCategoriaPorIdAsync(int id);
    Task<bool> ExisteDescripcionAsync(string descripcion, int idCategoria, int? excludeId = null);
    Task<IEnumerable<Categoria>> ObtenerCategoriasAsync();
    Task CreateAsync(SubCategoria subCategoria);
    Task UpdateAsync(SubCategoria subCategoria);
    Task DeleteAsync(int id);
}
