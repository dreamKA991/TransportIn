using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class RoadRestriction
{
    public int Id { get; set; }

    public string RestrictionType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Address { get; set; } = null!;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public double? MaxWeightTons { get; set; }

    public double? MaxHeightMeters { get; set; }

    public double? MaxWidthMeters { get; set; }

    public double? MaxLengthMeters { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public bool IsActive { get; set; }
}
