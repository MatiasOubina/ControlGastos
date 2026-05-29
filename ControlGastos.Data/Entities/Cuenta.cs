using System;
using System.Collections.Generic;

namespace ControlGastos.Data.Entities;

public partial class Cuenta
{
    public int Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();

    public virtual ICollection<SaldosIniciale> SaldosIniciales { get; set; } = new List<SaldosIniciale>();
}
