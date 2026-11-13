using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class DriverReport
{
    public int Id { get; set; }

    public int DriverId { get; set; }

    public int? OrderId { get; set; }

    public int? RouteId { get; set; }

    public string ReportType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string CargoCondition { get; set; } = null!;

    public string VehicleCondition { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool RequiresAttention { get; set; }

    public virtual Employee Driver { get; set; } = null!;

    public virtual Order? Order { get; set; }

    public virtual Route? Route { get; set; }
}
