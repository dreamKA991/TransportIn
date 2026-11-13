using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class OrderAssignment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int VehicleId { get; set; }

    public int DriverId { get; set; }

    public DateTime AssignedAt { get; set; }

    public DateTime PlannedStart { get; set; }

    public DateTime PlannedEnd { get; set; }

    public DateTime? ActualStart { get; set; }

    public DateTime? ActualEnd { get; set; }

    public string Status { get; set; } = null!;

    public string Notes { get; set; } = null!;

    public virtual Employee Driver { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual Vehicle Vehicle { get; set; } = null!;
}
