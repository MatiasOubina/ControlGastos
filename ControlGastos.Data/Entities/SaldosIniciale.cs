using System;
using System.Collections.Generic;

namespace ControlGastos.Data.Entities;

public partial class SaldosIniciale
{
    public int Id { get; set; }

    public int IdCuenta { get; set; }

    public byte Mes { get; set; }

    public short Anio { get; set; }

    public decimal Monto { get; set; }

    public virtual Cuenta IdCuentaNavigation { get; set; } = null!;
}
