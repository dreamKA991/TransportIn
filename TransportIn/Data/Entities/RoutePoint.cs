using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class RoutePoint
{
    public int Id { get; set; }

    public int RouteId { get; set; }

    public int SequenceNumber { get; set; }

    public string PointType { get; set; } = null!;

    public string Address { get; set; } = null!;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public DateTime? PlannedArrival { get; set; }

    public DateTime? ActualArrival { get; set; }

    public string Notes { get; set; } = null!;

    public virtual Route Route { get; set; } = null!;
}
