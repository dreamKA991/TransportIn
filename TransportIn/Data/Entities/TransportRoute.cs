using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class TransportRoute
{
    public int Id { get; set; }

    public int DriverId { get; set; }

    public int CargoId { get; set; }

    public string Status { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}
