using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class ActivityLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? EmployeeId { get; set; }

    public string Action { get; set; } = null!;

    public string EntityName { get; set; } = null!;

    public int? EntityId { get; set; }

    public string Description { get; set; } = null!;

    public string IpAddress { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual User? User { get; set; }
}
