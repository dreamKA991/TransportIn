using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class RoadEvent
{
    public int Id { get; set; }

    public string EventType { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Address { get; set; } = null!;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public string Severity { get; set; } = null!;

    public string? WeatherCondition { get; set; }

    public double? WindSpeedMs { get; set; }

    public double? VisibilityMeters { get; set; }

    public double? TemperatureC { get; set; }

    public bool IsActive { get; set; }
}
