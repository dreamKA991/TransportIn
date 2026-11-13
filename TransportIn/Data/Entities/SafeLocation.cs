using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class SafeLocation
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string LocationType { get; set; } = null!;

    public string Address { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int? Capacity { get; set; }

    public bool HasSecurity { get; set; }

    public bool HasParking { get; set; }

    public bool HasFuel { get; set; }

    public bool HasRepairService { get; set; }

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; }
}
