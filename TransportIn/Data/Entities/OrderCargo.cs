using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class OrderCargo
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int CargoId { get; set; }

    public int Quantity { get; set; }

    public string Notes { get; set; } = null!;

    public virtual Cargo Cargo { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
