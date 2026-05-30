using ControlGastos.Data.Context;
using ControlGastos.Data.Entities;
using ControlGastos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Services.Services;

public class MovimientoService : IMovimientoService
{
    private readonly ControlGastosDbContext _context;

    public MovimientoService(ControlGastosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Movimiento>> ObtenerPorPeriodoAsync(int mes, int anio, int? idCuenta = null)
    {
        var query = _context.Movimientos
            .Include(m => m.IdCuentaNavigation)
            .Include(m => m.IdCategoriaNavigation)
            .Include(m => m.IdSubCategoriaNavigation)
            .Include(m => m.IdTipoMovimientoNavigation)
            .Include(m => m.IdFormaDePagoNavigation)
            .Include(m => m.IdMovimientoVinculadoNavigation)
                .ThenInclude(mv => mv!.IdCuentaNavigation)
            .Where(m => m.FechaMovimiento.Month == mes && m.FechaMovimiento.Year == anio);

        if (idCuenta.HasValue)
            query = query.Where(m => m.IdCuenta == idCuenta.Value);

        return await query
            .OrderByDescending(m => m.FechaMovimiento)
            .ThenByDescending(m => m.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Movimiento>> ObtenerPorRangoAsync(DateOnly desde, DateOnly hasta)
        => await _context.Movimientos
            .Include(m => m.IdCuentaNavigation)
            .Include(m => m.IdCategoriaNavigation)
            .Include(m => m.IdTipoMovimientoNavigation)
            .Include(m => m.IdFormaDePagoNavigation)
            .Include(m => m.IdSubCategoriaNavigation)
            .Where(m => m.FechaMovimiento >= desde && m.FechaMovimiento <= hasta)
            .OrderByDescending(m => m.FechaMovimiento)
            .ToListAsync();

    public async Task<Movimiento?> ObtenerPorIdAsync(int id)
        => await _context.Movimientos
            .Include(m => m.IdCuentaNavigation)
            .Include(m => m.IdCategoriaNavigation)
            .Include(m => m.IdSubCategoriaNavigation)
            .Include(m => m.IdTipoMovimientoNavigation)
            .Include(m => m.IdFormaDePagoNavigation)
            .Include(m => m.IdMovimientoVinculadoNavigation)
                .ThenInclude(mv => mv!.IdCuentaNavigation)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<IEnumerable<Cuenta>> ObtenerCuentasAsync()
        => await _context.Cuentas.OrderBy(c => c.Descripcion).ToListAsync();

    public async Task<IEnumerable<Categoria>> ObtenerCategoriasAsync()
        => await _context.Categorias.OrderBy(c => c.Descripcion).ToListAsync();

    public async Task<IEnumerable<SubCategoria>> ObtenerSubCategoriasPorCategoriaAsync(int idCategoria)
        => await _context.SubCategorias
            .Where(s => s.IdCategoria == idCategoria)
            .OrderBy(s => s.Descripcion)
            .ToListAsync();

    public async Task<IEnumerable<TiposMovimiento>> ObtenerTiposMovimientoAsync()
        => await _context.TiposMovimientos.OrderBy(t => t.Descripcion).ToListAsync();

    public async Task<IEnumerable<FormasDePago>> ObtenerFormasDePagoAsync()
        => await _context.FormasDePagos.OrderBy(f => f.Descripcion).ToListAsync();

    // ── CRUD genérico ────────────────────────────────────────────────────────

    public async Task CreateAsync(Movimiento movimiento)
    {
        _context.Movimientos.Add(movimiento);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Actualiza un movimiento regular.
    /// Si tenía un movimiento vinculado (era un pasaje que se convirtió a otro tipo),
    /// elimina el movimiento vinculado y rompe el vínculo.
    /// </summary>
    public async Task UpdateAsync(Movimiento movimiento)
    {
        if (movimiento.IdMovimientoVinculado.HasValue)
        {
            var vinculado = await _context.Movimientos.FindAsync(movimiento.IdMovimientoVinculado.Value);
            if (vinculado != null)
            {
                vinculado.IdMovimientoVinculado = null;
                await _context.SaveChangesAsync();
                _context.Movimientos.Remove(vinculado);
            }
            movimiento.IdMovimientoVinculado = null;
        }

        _context.Movimientos.Update(movimiento);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina un movimiento. Si es un pasaje, elimina también el movimiento vinculado.
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var movimiento = await _context.Movimientos.FindAsync(id);
        if (movimiento == null) return;

        if (movimiento.IdMovimientoVinculado.HasValue)
        {
            var vinculado = await _context.Movimientos.FindAsync(movimiento.IdMovimientoVinculado.Value);
            if (vinculado != null)
            {
                // Romper referencias circulares antes de eliminar
                movimiento.IdMovimientoVinculado = null;
                vinculado.IdMovimientoVinculado  = null;
                await _context.SaveChangesAsync();
                _context.Movimientos.Remove(vinculado);
            }
        }

        _context.Movimientos.Remove(movimiento);
        await _context.SaveChangesAsync();
    }

    // ── Pasaje entre cuentas ─────────────────────────────────────────────────

    /// <summary>
    /// Devuelve el Id de la categoría "Pasaje entre cuentas".
    /// Si no existe, la crea automáticamente.
    /// </summary>
    public async Task<int> ObtenerIdCategoriaPasajeAsync()
    {
        var cat = await _context.Categorias.FirstOrDefaultAsync(c =>
            c.Descripcion.ToLower().Contains("pasaje")       ||
            c.Descripcion.ToLower().Contains("transferencia") ||
            c.Descripcion.ToLower().Contains("traspaso"));

        if (cat != null) return cat.Id;

        // Crear automáticamente si no existe
        var nueva = new Categoria { Descripcion = "Pasaje entre cuentas" };
        _context.Categorias.Add(nueva);
        await _context.SaveChangesAsync();
        return nueva.Id;
    }

    /// <summary>
    /// Crea dos movimientos vinculados para un pasaje entre cuentas:
    /// - Movimiento origen  (cuenta fuente)
    /// - Movimiento destino (cuenta destino)
    /// Ambos se referencian mutuamente con IdMovimientoVinculado.
    /// </summary>
    public async Task CreatePasajeAsync(Movimiento origen, int idCuentaDestino)
    {
        using var tx = await _context.Database.BeginTransactionAsync();

        var idCat = await ObtenerIdCategoriaPasajeAsync();
        origen.IdCategoria    = idCat;
        origen.IdSubCategoria = null;

        // 1. Crear movimiento origen
        _context.Movimientos.Add(origen);
        await _context.SaveChangesAsync();

        // 2. Crear movimiento destino apuntando al origen
        var destino = new Movimiento
        {
            FechaMovimiento       = origen.FechaMovimiento,
            IdCuenta              = idCuentaDestino,
            IdCategoria           = idCat,
            IdSubCategoria        = null,
            Detalle               = origen.Detalle,
            Monto                 = origen.Monto,
            IdTipoMovimiento      = origen.IdTipoMovimiento,
            IdFormaDePago         = origen.IdFormaDePago,
            IdMovimientoVinculado = origen.Id
        };
        _context.Movimientos.Add(destino);
        await _context.SaveChangesAsync();

        // 3. Referenciar origen → destino (vínculo bidireccional)
        origen.IdMovimientoVinculado = destino.Id;
        await _context.SaveChangesAsync();

        await tx.CommitAsync();
    }

    /// <summary>
    /// Actualiza ambos movimientos de un pasaje.
    /// Si el movimiento no tenía un vinculado (era regular y se convirtió a pasaje),
    /// crea el movimiento destino automáticamente.
    /// </summary>
    public async Task UpdatePasajeAsync(Movimiento movimiento, int idCuentaDestino)
    {
        var idCat = await ObtenerIdCategoriaPasajeAsync();
        movimiento.IdCategoria    = idCat;
        movimiento.IdSubCategoria = null;

        if (movimiento.IdMovimientoVinculado.HasValue)
        {
            // Ya era un pasaje — actualizar el movimiento vinculado existente
            var vinculado = await _context.Movimientos.FindAsync(movimiento.IdMovimientoVinculado.Value);
            if (vinculado != null)
            {
                vinculado.FechaMovimiento  = movimiento.FechaMovimiento;
                vinculado.IdCuenta         = idCuentaDestino;
                vinculado.Monto            = movimiento.Monto;
                vinculado.IdFormaDePago    = movimiento.IdFormaDePago;
                vinculado.Detalle          = movimiento.Detalle;
                vinculado.IdTipoMovimiento = movimiento.IdTipoMovimiento;
                vinculado.IdCategoria      = idCat;
                vinculado.IdSubCategoria   = null;
                _context.Movimientos.Update(vinculado);
            }
        }
        else
        {
            // Era un movimiento regular que se convierte a pasaje — crear movimiento destino
            var nuevo = new Movimiento
            {
                FechaMovimiento       = movimiento.FechaMovimiento,
                IdCuenta              = idCuentaDestino,
                IdCategoria           = idCat,
                Detalle               = movimiento.Detalle,
                Monto                 = movimiento.Monto,
                IdTipoMovimiento      = movimiento.IdTipoMovimiento,
                IdFormaDePago         = movimiento.IdFormaDePago,
                IdMovimientoVinculado = movimiento.Id
            };
            _context.Movimientos.Add(nuevo);
            await _context.SaveChangesAsync();
            movimiento.IdMovimientoVinculado = nuevo.Id;
        }

        _context.Movimientos.Update(movimiento);
        await _context.SaveChangesAsync();
    }
}
