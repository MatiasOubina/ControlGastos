using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControlGastos.Web.Controllers;

public class SaldosInicialesController : Controller
{
    private readonly ISaldosInicialesService _service;

    public SaldosInicialesController(ISaldosInicialesService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(int? mes, int? anio)
    {
        int mesFiltro  = mes  ?? DateTime.Today.Month;
        int anioFiltro = anio ?? DateTime.Today.Year;

        var saldos = await _service.ObtenerPorPeriodoAsync(mesFiltro, anioFiltro);

        ViewBag.MesFiltro  = mesFiltro;
        ViewBag.AnioFiltro = anioFiltro;
        ViewBag.TotalSaldo = saldos.Sum(s => s.Monto);

        return View(saldos);
    }

    public async Task<IActionResult> Create(int? mes, int? anio)
    {
        var saldo = new SaldosIniciale
        {
            Mes  = (byte)(mes  ?? DateTime.Today.Month),
            Anio = (short)(anio ?? DateTime.Today.Year)
        };

        await CargarCuentasAsync();
        return View(saldo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaldosIniciale saldo)
    {
        ModelState.Remove(nameof(SaldosIniciale.IdCuentaNavigation));

        if (!ModelState.IsValid)
        {
            await CargarCuentasAsync(saldo.IdCuenta);
            return View(saldo);
        }

        if (await _service.ExistePeriodoAsync(saldo.IdCuenta, saldo.Mes, saldo.Anio))
        {
            ModelState.AddModelError(string.Empty,
                "Ya existe un saldo inicial para esa cuenta en el período seleccionado.");
            await CargarCuentasAsync(saldo.IdCuenta);
            return View(saldo);
        }

        await _service.CreateAsync(saldo);
        TempData["Success"] = "Saldo inicial guardado correctamente.";
        return RedirectToAction(nameof(Index), new { mes = saldo.Mes, anio = saldo.Anio });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var saldo = await _service.ObtenerPorIdAsync(id);
        if (saldo == null) return NotFound();

        await CargarCuentasAsync(saldo.IdCuenta);
        return View(saldo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SaldosIniciale saldo)
    {
        if (id != saldo.Id) return BadRequest();

        ModelState.Remove(nameof(SaldosIniciale.IdCuentaNavigation));

        if (!ModelState.IsValid)
        {
            await CargarCuentasAsync(saldo.IdCuenta);
            return View(saldo);
        }

        if (await _service.ExistePeriodoAsync(saldo.IdCuenta, saldo.Mes, saldo.Anio, id))
        {
            ModelState.AddModelError(string.Empty,
                "Ya existe un saldo inicial para esa cuenta en el período seleccionado.");
            await CargarCuentasAsync(saldo.IdCuenta);
            return View(saldo);
        }

        await _service.UpdateAsync(saldo);
        TempData["Success"] = "Saldo inicial actualizado correctamente.";
        return RedirectToAction(nameof(Index), new { mes = saldo.Mes, anio = saldo.Anio });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var saldo = await _service.ObtenerPorIdAsync(id);
        int mes  = saldo?.Mes  ?? DateTime.Today.Month;
        int anio = saldo?.Anio ?? DateTime.Today.Year;

        await _service.DeleteAsync(id);
        TempData["Success"] = "Saldo inicial eliminado correctamente.";
        return RedirectToAction(nameof(Index), new { mes, anio });
    }

    private async Task CargarCuentasAsync(int? selectedId = null)
    {
        var cuentas = await _service.ObtenerCuentasAsync();
        ViewBag.Cuentas = new SelectList(cuentas, "Id", "Descripcion", selectedId);
    }
}
