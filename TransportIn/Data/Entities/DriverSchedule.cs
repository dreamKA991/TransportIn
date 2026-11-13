using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class DriverSchedule
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly WorkDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string ShiftType { get; set; } = null!;

    public bool IsAvailable { get; set; }

    public string Notes { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
