using Microsoft.AspNetCore.Authorization;
using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlGastos.Web.Controllers;

[Authorize]
public class TiposMovimientoController : Controller
{
    private readonly ITiposMovimientoService _service;

    public TiposMovimientoController(ITiposMovimientoService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var tipos = await _service.ObtenerTodosLosTiposMovimiento();
        return View(tipos);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TiposMovimiento tipoMovimiento)
    {
        ModelState.Remove(nameof(TiposMovimiento.Movimientos));

        if (!ModelState.IsValid) return View(tipoMovimiento);

        if (await _service.ExisteDescripcionAsync(tipoMovimiento.Descripcion))
        {
            ModelState.AddModelError(nameof(tipoMovimiento.Descripcion), "Ya existe un tipo de movimiento con esa descripciÃ³n.");
            return View(tipoMovimiento);
        }

        await _service.CreateAsync(tipoMovimiento);
        TempData["Success"] = "Tipo de movimiento creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var tipoMovimiento = await _service.ObtenerTipoMovimientoPorIdAsync(id);
        if (tipoMovimiento == null) return NotFound();
        return View(tipoMovimiento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TiposMovimiento tipoMovimiento)
    {
        if (id != tipoMovimiento.Id) return BadRequest();

        ModelState.Remove(nameof(TiposMovimiento.Movimientos));

        if (!ModelState.IsValid) return View(tipoMovimiento);

        if (await _service.ExisteDescripcionAsync(tipoMovimiento.Descripcion, id))
        {
            ModelState.AddModelError(nameof(tipoMovimiento.Descripcion), "Ya existe un tipo de movimiento con esa descripciÃ³n.");
            return View(tipoMovimiento);
        }

        await _service.UpdateAsync(tipoMovimiento);
        TempData["Success"] = "Tipo de movimiento actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Tipo de movimiento eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}

