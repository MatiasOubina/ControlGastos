using Microsoft.AspNetCore.Authorization;
using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControlGastos.Web.Controllers;

[Authorize]
public class SubCategoriaController : Controller
{
    private readonly ISubCategoriaService _service;

    public SubCategoriaController(ISubCategoriaService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var subCategorias = await _service.ObtenerTodasLasSubCategorias();
        return View(subCategorias);
    }

    public async Task<IActionResult> Create()
    {
        await CargarCategoriasAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubCategoria subCategoria)
    {
        ModelState.Remove(nameof(SubCategoria.IdCategoriaNavigation));

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(subCategoria.IdCategoria);
            return View(subCategoria);
        }

        if (await _service.ExisteDescripcionAsync(subCategoria.Descripcion, subCategoria.IdCategoria))
        {
            ModelState.AddModelError(nameof(subCategoria.Descripcion),
                "Ya existe una subcategorÃ­a con esa descripciÃ³n en la categorÃ­a seleccionada.");
            await CargarCategoriasAsync(subCategoria.IdCategoria);
            return View(subCategoria);
        }

        await _service.CreateAsync(subCategoria);
        TempData["Success"] = "SubcategorÃ­a creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var subCategoria = await _service.ObtenerSubCategoriaPorIdAsync(id);
        if (subCategoria == null) return NotFound();

        await CargarCategoriasAsync(subCategoria.IdCategoria);
        return View(subCategoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SubCategoria subCategoria)
    {
        if (id != subCategoria.Id) return BadRequest();

        ModelState.Remove(nameof(SubCategoria.IdCategoriaNavigation));

        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync(subCategoria.IdCategoria);
            return View(subCategoria);
        }

        if (await _service.ExisteDescripcionAsync(subCategoria.Descripcion, subCategoria.IdCategoria, id))
        {
            ModelState.AddModelError(nameof(subCategoria.Descripcion),
                "Ya existe una subcategorÃ­a con esa descripciÃ³n en la categorÃ­a seleccionada.");
            await CargarCategoriasAsync(subCategoria.IdCategoria);
            return View(subCategoria);
        }

        await _service.UpdateAsync(subCategoria);
        TempData["Success"] = "SubcategorÃ­a actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "SubcategorÃ­a eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private async Task CargarCategoriasAsync(int? selectedId = null)
    {
        var categorias = await _service.ObtenerCategoriasAsync();
        ViewBag.Categorias = new SelectList(categorias, "Id", "Descripcion", selectedId);
    }
}

