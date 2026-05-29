using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlGastos.Web.Controllers;

public class FormasDePagoController : Controller
{
    private readonly IFormasDePagoService _service;

    public FormasDePagoController(IFormasDePagoService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var formas = await _service.ObtenerTodasLasFormasDePago();
        return View(formas);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FormasDePago formaDePago)
    {
        ModelState.Remove(nameof(FormasDePago.Movimientos));

        if (!ModelState.IsValid) return View(formaDePago);

        if (await _service.ExisteDescripcionAsync(formaDePago.Descripcion))
        {
            ModelState.AddModelError(nameof(formaDePago.Descripcion), "Ya existe una forma de pago con esa descripción.");
            return View(formaDePago);
        }

        await _service.CreateAsync(formaDePago);
        TempData["Success"] = "Forma de pago creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var formaDePago = await _service.ObtenerFormaDePagoPorIdAsync(id);
        if (formaDePago == null) return NotFound();
        return View(formaDePago);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FormasDePago formaDePago)
    {
        if (id != formaDePago.Id) return BadRequest();

        ModelState.Remove(nameof(FormasDePago.Movimientos));

        if (!ModelState.IsValid) return View(formaDePago);

        if (await _service.ExisteDescripcionAsync(formaDePago.Descripcion, id))
        {
            ModelState.AddModelError(nameof(formaDePago.Descripcion), "Ya existe una forma de pago con esa descripción.");
            return View(formaDePago);
        }

        await _service.UpdateAsync(formaDePago);
        TempData["Success"] = "Forma de pago actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Forma de pago eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
