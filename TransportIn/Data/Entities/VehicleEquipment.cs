using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class VehicleEquipment
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Quantity { get; set; }

    public bool IsWorking { get; set; }

    public virtual Vehicle Vehicle { get; set; } = null!;
}
