using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class OrderStatusHistory
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string PreviousStatus { get; set; } = null!;

    public string NewStatus { get; set; } = null!;

    public DateTime ChangedAt { get; set; }

    public int? ChangedByEmployeeId { get; set; }

    public string Comment { get; set; } = null!;

    public virtual Employee? ChangedByEmployee { get; set; }

    public virtual Order Order { get; set; } = null!;
}
