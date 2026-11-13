using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class VehicleDocument
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public int DocumentType { get; set; }

    public string DocumentNumber { get; set; } = null!;

    public DateOnly IssueDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string IssuedBy { get; set; } = null!;

    public string Details { get; set; } = null!;

    public string? Status { get; set; }

    public string? FilePath { get; set; }

    public virtual Vehicle Vehicle { get; set; } = null!;
}
