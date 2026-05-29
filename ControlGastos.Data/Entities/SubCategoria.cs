using System;
using System.Collections.Generic;

namespace ControlGastos.Data.Entities;

public partial class SubCategoria
{
    public int Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public int IdCategoria { get; set; }

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
