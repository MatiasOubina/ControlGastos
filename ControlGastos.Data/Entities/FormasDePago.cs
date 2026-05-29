using System;
using System.Collections.Generic;

namespace ControlGastos.Data.Entities;

public partial class FormasDePago
{
    public int Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
