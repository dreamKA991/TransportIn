using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class OrderPoint
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int? ContactPersonId { get; set; }

    public string PointType { get; set; } = null!;

    public string Address { get; set; } = null!;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string ContactName { get; set; } = null!;

    public string ContactPhone { get; set; } = null!;

    public int SequenceNumber { get; set; }

    public DateTime? PlannedArrival { get; set; }

    public DateTime? ActualArrival { get; set; }

    public string Status { get; set; } = null!;

    public string Notes { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual ContactPerson? ContactPersonNavigation { get; set; }
}
