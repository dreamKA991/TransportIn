using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class Employee
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string Role { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string LicenseCategory { get; set; } = null!;

    public string MedicalCertExpiry { get; set; } = null!;

    public bool IsActive { get; set; }

    public string Address { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? TaxId { get; set; }

    public string PhotoPath { get; set; } = null!;

    public int RoleId { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public virtual ICollection<DriverReport> DriverReports { get; set; } = new List<DriverReport>();

    public virtual ICollection<DriverSchedule> DriverSchedules { get; set; } = new List<DriverSchedule>();

    public virtual ICollection<EmployeeDocument> EmployeeDocuments { get; set; } = new List<EmployeeDocument>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<OrderAssignment> OrderAssignments { get; set; } = new List<OrderAssignment>();

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual Role RoleNavigation { get; set; } = null!;

    public virtual User? UserNavigation { get; set; }

    public virtual ICollection<RouteHistory> RouteHistories { get; set; } = new List<RouteHistory>();

    public virtual ICollection<RuleExecutionHistory> RuleExecutionHistories { get; set; } = new List<RuleExecutionHistory>();
}
