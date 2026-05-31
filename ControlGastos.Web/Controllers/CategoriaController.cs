using Microsoft.AspNetCore.Authorization;
using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlGastos.Web.Controllers;

[Authorize]
public class CategoriaController : Controller
{
    private readonly ICategoriaService _service;

    public CategoriaController(ICategoriaService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var categorias = await _service.ObtenerTodasLasCategorias();
        return View(categorias);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        ModelState.Remove(nameof(Categoria.SubCategoria));
        ModelState.Remove(nameof(Categoria.Movimientos));

        if (!ModelState.IsValid) return View(categoria);

        if (await _service.ExisteDescripcionAsync(categoria.Descripcion))
        {
            ModelState.AddModelError(nameof(categoria.Descripcion), "Ya existe una categorÃ­a con esa descripciÃ³n.");
            return View(categoria);
        }

        await _service.CreateAsync(categoria);
        TempData["Success"] = "CategorÃ­a creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var categoria = await _service.ObtenerCategoriaPorIdAsync(id);
        if (categoria == null) return NotFound();
        return View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Categoria categoria)
    {
        if (id != categoria.Id) return BadRequest();

        ModelState.Remove(nameof(Categoria.SubCategoria));
        ModelState.Remove(nameof(Categoria.Movimientos));

        if (!ModelState.IsValid) return View(categoria);

        if (await _service.ExisteDescripcionAsync(categoria.Descripcion, id))
        {
            ModelState.AddModelError(nameof(categoria.Descripcion), "Ya existe una categorÃ­a con esa descripciÃ³n.");
            return View(categoria);
        }

        await _service.UpdateAsync(categoria);
        TempData["Success"] = "CategorÃ­a actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "CategorÃ­a eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}

