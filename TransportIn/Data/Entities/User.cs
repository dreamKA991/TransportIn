using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int Role { get; set; }

    public int? EmployeeId { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public virtual Employee? EmployeeNavigation { get; set; }

    public virtual Role RoleNavigation { get; set; } = null!;

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}
