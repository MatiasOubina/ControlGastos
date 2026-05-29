using System;
using System.Collections.Generic;

namespace ControlGastos.Data.Entities;

public partial class Movimiento
{
    public int Id { get; set; }

    public DateOnly FechaMovimiento { get; set; }

    public int IdCuenta { get; set; }

    public int IdCategoria { get; set; }

    public int? IdSubCategoria { get; set; }

    public string? Detalle { get; set; }

    public decimal Monto { get; set; }

    public int IdTipoMovimiento { get; set; }

    public int IdFormaDePago { get; set; }

    public int? IdMovimientoVinculado { get; set; }

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual Cuenta IdCuentaNavigation { get; set; } = null!;

    public virtual FormasDePago IdFormaDePagoNavigation { get; set; } = null!;

    public virtual Movimiento? IdMovimientoVinculadoNavigation { get; set; }

    public virtual SubCategoria? IdSubCategoriaNavigation { get; set; }

    public virtual TiposMovimiento IdTipoMovimientoNavigation { get; set; } = null!;

    public virtual ICollection<Movimiento> InverseIdMovimientoVinculadoNavigation { get; set; } = new List<Movimiento>();
}
