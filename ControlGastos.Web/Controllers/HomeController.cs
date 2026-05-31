using Microsoft.AspNetCore.Authorization;
using ControlGastos.Services.Interfaces;
using ControlGastos.Web.Helpers;
using ControlGastos.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace ControlGastos.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IMovimientoService      _movService;
    private readonly ISaldosInicialesService _saldosService;

    public HomeController(IMovimientoService movService, ISaldosInicialesService saldosService)
    {
        _movService    = movService;
        _saldosService = saldosService;
    }

    public async Task<IActionResult> Index()
    {
        var hoy  = DateTime.Today;
        var mes  = hoy.Month;
        var anio = hoy.Year;

        // â”€â”€ Consultas â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var movsMes       = (await _movService.ObtenerPorPeriodoAsync(mes, anio)).ToList();
        var saldosInic    = (await _saldosService.ObtenerPorPeriodoAsync(mes, anio)).ToList();
        var cuentas       = (await _movService.ObtenerCuentasAsync()).ToList();

        // Rango de 6 meses para el grÃ¡fico de evoluciÃ³n
        var desde = DateOnly.FromDateTime(hoy.AddMonths(-5).AddDays(1 - hoy.Day)); // primer dÃ­a de hace 5 meses
        var hasta = DateOnly.FromDateTime(hoy);
        var movsRango = (await _movService.ObtenerPorRangoAsync(desde, hasta)).ToList();

        // â”€â”€ ViewModel â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        var vm = new DashboardViewModel { Mes = mes, Anio = anio };

        // KPIs del mes actual
        vm.TotalIngresos = movsMes
            .Where(m => MovimientoHelper.GetClasificacion(m.IdTipoMovimientoNavigation.Descripcion) == "Ingreso")
            .Sum(m => m.Monto);
        vm.TotalEgresos = movsMes
            .Where(m => MovimientoHelper.GetClasificacion(m.IdTipoMovimientoNavigation.Descripcion) == "Egreso")
            .Sum(m => m.Monto);
        vm.BalanceNeto         = vm.TotalIngresos - vm.TotalEgresos;
        vm.CantidadMovimientos = movsMes.Count;

        // Ãšltimos 5 movimientos
        vm.UltimosMovimientos = movsMes.Take(5).ToList();

        // Gastos por categorÃ­a (mes actual, solo egresos)
        vm.GastosPorCategoria = movsMes
            .Where(m => MovimientoHelper.GetClasificacion(m.IdTipoMovimientoNavigation.Descripcion) == "Egreso")
            .GroupBy(m => m.IdCategoriaNavigation.Descripcion)
            .Select(g => new ResumenCategoria(g.Key, g.Sum(m => m.Monto)))
            .OrderByDescending(x => x.Total)
            .Take(6)
            .ToList();

        // EvoluciÃ³n mensual (Ãºltimos 6 meses)
        var culturaEs = new CultureInfo("es-AR");
        for (int i = 5; i >= 0; i--)
        {
            var fecha = hoy.AddMonths(-i);
            var movs  = movsRango
                .Where(m => m.FechaMovimiento.Month == fecha.Month && m.FechaMovimiento.Year == fecha.Year)
                .ToList();

            var ing = movs
                .Where(m => MovimientoHelper.GetClasificacion(m.IdTipoMovimientoNavigation.Descripcion) == "Ingreso")
                .Sum(m => m.Monto);
            var egr = movs
                .Where(m => MovimientoHelper.GetClasificacion(m.IdTipoMovimientoNavigation.Descripcion) == "Egreso")
                .Sum(m => m.Monto);

            var nombreMes = culturaEs.DateTimeFormat.GetAbbreviatedMonthName(fecha.Month);
            nombreMes = char.ToUpper(nombreMes[0]) + nombreMes[1..].TrimEnd('.');

            vm.EvolucionMensual.Add(new ResumenMensual(nombreMes, ing, egr));
        }

        // Saldo por cuenta = saldo inicial + movimientos del mes
        foreach (var cuenta in cuentas)
        {
            var saldoInicial = saldosInic
                .FirstOrDefault(s => s.IdCuenta == cuenta.Id)?.Monto ?? 0m;

            decimal movBalance = 0m;
            foreach (var m in movsMes.Where(m => m.IdCuenta == cuenta.Id))
            {
                var clasif = MovimientoHelper.GetClasificacion(m.IdTipoMovimientoNavigation.Descripcion);
                switch (clasif)
                {
                    case "Ingreso":
                        movBalance += m.Monto;
                        break;
                    case "Egreso":
                        movBalance -= m.Monto;
                        break;
                    case "Pasaje":
                        // ConvenciÃ³n: ID menor = origen (dÃ©bito), ID mayor = destino (crÃ©dito)
                        if (m.IdMovimientoVinculado.HasValue)
                            movBalance += m.Id < m.IdMovimientoVinculado.Value ? -m.Monto : m.Monto;
                        break;
                }
            }

            vm.SaldosPorCuenta.Add(new SaldoCuenta(cuenta.Descripcion, saldoInicial + movBalance, 0));
        }

        // Calcular porcentaje relativo al saldo mÃ¡ximo positivo
        var maxSaldo = vm.SaldosPorCuenta.Any() ? vm.SaldosPorCuenta.Max(s => s.Saldo) : 0m;
        if (maxSaldo > 0)
        {
            vm.SaldosPorCuenta = vm.SaldosPorCuenta
                .Select(s => new SaldoCuenta(
                    s.Nombre,
                    s.Saldo,
                    (int)Math.Max(0, Math.Round(s.Saldo / maxSaldo * 100))))
                .ToList();
        }

        return View(vm);
    }
}

