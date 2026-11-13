using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class Route
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int VehicleId { get; set; }

    public string StartPoint { get; set; } = null!;

    public string EndPoint { get; set; } = null!;

    public double DistanceKm { get; set; }

    public int EstimatedDurationMinutes { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<DriverReport> DriverReports { get; set; } = new List<DriverReport>();

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<RouteHistory> RouteHistories { get; set; } = new List<RouteHistory>();

    public virtual ICollection<RoutePoint> RoutePoints { get; set; } = new List<RoutePoint>();

    public virtual Vehicle Vehicle { get; set; } = null!;
}
