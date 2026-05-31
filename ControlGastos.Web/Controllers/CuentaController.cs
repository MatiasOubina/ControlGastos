using Microsoft.AspNetCore.Authorization;
using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlGastos.Web.Controllers;

[Authorize]
public class CuentaController : Controller
{
    private readonly ICuentaService _service;

    public CuentaController(ICuentaService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var cuentas = await _service.ObtenerTodasLasCuentas();
        return View(cuentas);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cuenta cuenta)
    {
        ModelState.Remove(nameof(Cuenta.Movimientos));
        ModelState.Remove(nameof(Cuenta.SaldosIniciales));

        if (!ModelState.IsValid) return View(cuenta);

        if (await _service.ExisteDescripcionAsync(cuenta.Descripcion))
        {
            ModelState.AddModelError(nameof(cuenta.Descripcion), "Ya existe una cuenta con esa descripciÃ³n.");
            return View(cuenta);
        }

        await _service.CreateAsync(cuenta);
        TempData["Success"] = "Cuenta creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var cuenta = await _service.ObtenerCuentaPorIdAsync(id);
        if (cuenta == null) return NotFound();
        return View(cuenta);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Cuenta cuenta)
    {
        if (id != cuenta.Id) return BadRequest();

        ModelState.Remove(nameof(Cuenta.Movimientos));
        ModelState.Remove(nameof(Cuenta.SaldosIniciales));

        if (!ModelState.IsValid) return View(cuenta);

        if (await _service.ExisteDescripcionAsync(cuenta.Descripcion, id))
        {
            ModelState.AddModelError(nameof(cuenta.Descripcion), "Ya existe una cuenta con esa descripciÃ³n.");
            return View(cuenta);
        }

        await _service.UpdateAsync(cuenta);
        TempData["Success"] = "Cuenta actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Cuenta eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}

