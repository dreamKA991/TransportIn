using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class VehicleLocation
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double? SpeedKmh { get; set; }

    public double? DirectionDegrees { get; set; }

    public DateTime RecordedAt { get; set; }

    public virtual Vehicle Vehicle { get; set; } = null!;
}
