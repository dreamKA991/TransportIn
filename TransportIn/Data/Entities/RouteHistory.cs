using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class RouteHistory
{
    public int Id { get; set; }

    public int RouteId { get; set; }

    public string PreviousStatus { get; set; } = null!;

    public string NewStatus { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime ChangedAt { get; set; }

    public int? ChangedByEmployeeId { get; set; }

    public virtual Employee? ChangedByEmployee { get; set; }

    public virtual Route Route { get; set; } = null!;
}
