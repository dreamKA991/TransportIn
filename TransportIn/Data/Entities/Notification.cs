using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class Notification
{
    public int Id { get; set; }

    public int? EmployeeId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string NotificationType { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual Employee? Employee { get; set; }
}
