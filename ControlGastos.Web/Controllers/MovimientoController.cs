using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using ControlGastos.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace ControlGastos.Web.Controllers;

public class MovimientoController : Controller
{
    private readonly IMovimientoService _service;

    public MovimientoController(IMovimientoService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(int? mes, int? anio, int? idCuenta)
    {
        int mesFiltro  = mes  ?? DateTime.Today.Month;
        int anioFiltro = anio ?? DateTime.Today.Year;

        // Para pasajes se muestran solo el movimiento origen (ID menor del par vinculado)
        var movimientos = (await _service.ObtenerPorPeriodoAsync(mesFiltro, anioFiltro, idCuenta))
            .Where(m => !(m.IdMovimientoVinculado.HasValue && m.Id > m.IdMovimientoVinculado.Value))
            .ToList();

        var tipos = await _service.ObtenerTiposMovimientoAsync();
        var cuentas = await _service.ObtenerCuentasAsync();

        // Totales con signo
        decimal totalIngresos = movimientos
            .Where(m => MovimientoHelper.GetClasificacion(m.IdTipoMovimientoNavigation.Descripcion) == "Ingreso")
            .Sum(m => m.Monto);
        decimal totalEgresos = movimientos
            .Where(m => MovimientoHelper.GetClasificacion(m.IdTipoMovimientoNavigation.Descripcion) == "Egreso")
            .Sum(m => m.Monto);

        ViewBag.MesFiltro     = mesFiltro;
        ViewBag.AnioFiltro    = anioFiltro;
        ViewBag.IdCuenta      = idCuenta;
        ViewBag.TotalIngresos = totalIngresos;
        ViewBag.TotalEgresos  = totalEgresos;
        ViewBag.Balance       = totalIngresos - totalEgresos;
        ViewBag.Cuentas       = new SelectList(cuentas, "Id", "Descripcion", idCuenta);

        return View(movimientos);
    }

    public async Task<IActionResult> Create()
    {
        var mov = new Movimiento { FechaMovimiento = DateOnly.FromDateTime(DateTime.Today) };
        await CargarDropdownsAsync(mov);
        return View(mov);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Movimiento movimiento, int? idCuentaDestino)
    {
        RemoverNavegaciones();

        // Determinar clasificación del tipo seleccionado
        var tipos = await _service.ObtenerTiposMovimientoAsync();
        var tipo  = tipos.FirstOrDefault(t => t.Id == movimiento.IdTipoMovimiento);
        bool esPasaje = tipo != null && MovimientoHelper.EsPasaje(tipo.Descripcion);

        if (esPasaje)
        {
            // Para pasajes la categoría la asigna el servicio automáticamente
            ModelState.Remove(nameof(Movimiento.IdCategoria));

            if (!idCuentaDestino.HasValue || idCuentaDestino == 0)
                ModelState.AddModelError("IdCuentaDestino", "Debe seleccionar la cuenta destino.");
            else if (idCuentaDestino == movimiento.IdCuenta)
                ModelState.AddModelError("IdCuentaDestino", "La cuenta destino no puede ser igual a la cuenta origen.");
        }

        if (!ModelState.IsValid)
        {
            await CargarDropdownsAsync(movimiento, idCuentaDestino);
            return View(movimiento);
        }

        if (esPasaje)
            await _service.CreatePasajeAsync(movimiento, idCuentaDestino!.Value);
        else
            await _service.CreateAsync(movimiento);

        TempData["Success"] = "Movimiento registrado correctamente.";
        return RedirectToAction(nameof(Index), new
        {
            mes  = movimiento.FechaMovimiento.Month,
            anio = movimiento.FechaMovimiento.Year
        });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var movimiento = await _service.ObtenerPorIdAsync(id);
        if (movimiento == null) return NotFound();

        var tipos   = await _service.ObtenerTiposMovimientoAsync();
        var tipo    = tipos.FirstOrDefault(t => t.Id == movimiento.IdTipoMovimiento);
        bool esPasaje = tipo != null && MovimientoHelper.EsPasaje(tipo.Descripcion);

        int? idCuentaDestino = esPasaje
            ? movimiento.IdMovimientoVinculadoNavigation?.IdCuenta
            : null;

        ViewBag.EsPasaje = esPasaje;
        await CargarDropdownsAsync(movimiento, idCuentaDestino);
        return View(movimiento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Movimiento movimiento, int? idCuentaDestino)
    {
        if (id != movimiento.Id) return BadRequest();

        RemoverNavegaciones();

        var tipos = await _service.ObtenerTiposMovimientoAsync();
        var tipo  = tipos.FirstOrDefault(t => t.Id == movimiento.IdTipoMovimiento);
        bool esPasaje = tipo != null && MovimientoHelper.EsPasaje(tipo.Descripcion);

        if (esPasaje)
        {
            ModelState.Remove(nameof(Movimiento.IdCategoria));

            if (!idCuentaDestino.HasValue || idCuentaDestino == 0)
                ModelState.AddModelError("IdCuentaDestino", "Debe seleccionar la cuenta destino.");
            else if (idCuentaDestino == movimiento.IdCuenta)
                ModelState.AddModelError("IdCuentaDestino", "La cuenta destino no puede ser igual a la cuenta origen.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.EsPasaje = esPasaje;
            await CargarDropdownsAsync(movimiento, idCuentaDestino);
            return View(movimiento);
        }

        if (esPasaje)
            await _service.UpdatePasajeAsync(movimiento, idCuentaDestino!.Value);
        else
            await _service.UpdateAsync(movimiento);

        TempData["Success"] = "Movimiento actualizado correctamente.";
        return RedirectToAction(nameof(Index), new
        {
            mes  = movimiento.FechaMovimiento.Month,
            anio = movimiento.FechaMovimiento.Year
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var mov = await _service.ObtenerPorIdAsync(id);
        int mes  = mov?.FechaMovimiento.Month ?? DateTime.Today.Month;
        int anio = mov?.FechaMovimiento.Year  ?? DateTime.Today.Year;

        await _service.DeleteAsync(id);
        TempData["Success"] = "Movimiento eliminado correctamente.";
        return RedirectToAction(nameof(Index), new { mes, anio });
    }

    // Endpoint AJAX: subcategorías por categoría
    [HttpGet]
    public async Task<IActionResult> GetSubCategorias(int idCategoria)
    {
        var subs = await _service.ObtenerSubCategoriasPorCategoriaAsync(idCategoria);
        return Json(subs.Select(s => new { s.Id, s.Descripcion }));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void RemoverNavegaciones()
    {
        ModelState.Remove(nameof(Movimiento.IdCategoriaNavigation));
        ModelState.Remove(nameof(Movimiento.IdCuentaNavigation));
        ModelState.Remove(nameof(Movimiento.IdFormaDePagoNavigation));
        ModelState.Remove(nameof(Movimiento.IdTipoMovimientoNavigation));
        ModelState.Remove(nameof(Movimiento.IdSubCategoriaNavigation));
        ModelState.Remove(nameof(Movimiento.IdMovimientoVinculadoNavigation));
        ModelState.Remove(nameof(Movimiento.InverseIdMovimientoVinculadoNavigation));
    }

    private async Task CargarDropdownsAsync(Movimiento? mov = null, int? idCuentaDestino = null)
    {
        var cuentas  = await _service.ObtenerCuentasAsync();
        var cats     = await _service.ObtenerCategoriasAsync();
        var tipos    = await _service.ObtenerTiposMovimientoAsync();
        var formas   = await _service.ObtenerFormasDePagoAsync();

        ViewBag.Cuentas         = new SelectList(cuentas, "Id", "Descripcion", mov?.IdCuenta);
        ViewBag.CuentasDestino  = new SelectList(cuentas, "Id", "Descripcion", idCuentaDestino);
        ViewBag.Categorias      = new SelectList(cats,   "Id", "Descripcion", mov?.IdCategoria);
        ViewBag.TiposMovimiento = new SelectList(tipos,  "Id", "Descripcion", mov?.IdTipoMovimiento);
        ViewBag.FormasDePago    = new SelectList(formas, "Id", "Descripcion", mov?.IdFormaDePago);

        // Subcategorías del movimiento actual (para Edit)
        if (mov?.IdCategoria > 0)
        {
            var subs = await _service.ObtenerSubCategoriasPorCategoriaAsync(mov.IdCategoria);
            ViewBag.SubCategorias = new SelectList(subs, "Id", "Descripcion", mov.IdSubCategoria);
        }
        else
        {
            ViewBag.SubCategorias = new SelectList(Enumerable.Empty<object>(), "Id", "Descripcion");
        }

        // Diccionario {tipoId → clasificación} para el JS del formulario
        ViewBag.TiposClasificacionJson = JsonSerializer.Serialize(
            tipos.ToDictionary(
                t => t.Id.ToString(),
                t => MovimientoHelper.GetClasificacion(t.Descripcion)
            ));
    }
}
