using ControlGastos.Data.Entities;

namespace ControlGastos.Web.Models;

public class DashboardViewModel
{
    public int Mes  { get; set; }
    public int Anio { get; set; }

    // ── KPIs del mes ────────────────────────────────────────────────────────
    public decimal TotalIngresos       { get; set; }
    public decimal TotalEgresos        { get; set; }
    public decimal BalanceNeto         { get; set; }
    public int     CantidadMovimientos { get; set; }

    // ── Evolución últimos 6 meses ─────────────────────────────────────────
    public List<ResumenMensual>   EvolucionMensual    { get; set; } = new();

    // ── Gastos por categoría (mes actual) ────────────────────────────────
    public List<ResumenCategoria> GastosPorCategoria  { get; set; } = new();

    // ── Últimos movimientos ───────────────────────────────────────────────
    public List<Movimiento>       UltimosMovimientos  { get; set; } = new();

    // ── Saldo por cuenta ─────────────────────────────────────────────────
    public List<SaldoCuenta>      SaldosPorCuenta     { get; set; } = new();
}

public record ResumenMensual(string NombreMes, decimal Ingresos, decimal Egresos);

public record ResumenCategoria(string Categoria, decimal Total);

public record SaldoCuenta(string Nombre, decimal Saldo, int PorcentajeRelativo);
