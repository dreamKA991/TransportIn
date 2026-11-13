using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TransportIn.Data.Entities;

namespace TransportIn.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Cargo> Cargos { get; set; }

    public virtual DbSet<ContactPerson> ContactPersons { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<DriverReport> DriverReports { get; set; }

    public virtual DbSet<DriverSchedule> DriverSchedules { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeDocument> EmployeeDocuments { get; set; }

    public virtual DbSet<ExpertRule> ExpertRules { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderAssignment> OrderAssignments { get; set; }

    public virtual DbSet<OrderCargo> OrderCargos { get; set; }

    public virtual DbSet<OrderPoint> OrderPoints { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public virtual DbSet<RoadEvent> RoadEvents { get; set; }

    public virtual DbSet<RoadRestriction> RoadRestrictions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TransportIn.Data.Entities.Route> Routes { get; set; }

    public virtual DbSet<RouteHistory> RouteHistories { get; set; }

    public virtual DbSet<RoutePoint> RoutePoints { get; set; }

    public virtual DbSet<RuleCondition> RuleConditions { get; set; }

    public virtual DbSet<RuleExecutionHistory> RuleExecutionHistories { get; set; }

    public virtual DbSet<SafeLocation> SafeLocations { get; set; }

    public virtual DbSet<TransportRoute> TransportRoutes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleDocument> VehicleDocuments { get; set; }

    public virtual DbSet<VehicleEquipment> VehicleEquipments { get; set; }

    public virtual DbSet<VehicleLocation> VehicleLocations { get; set; }

    public virtual DbSet<VehicleStatus> VehicleStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.EmployeeId).IsUnique();
            entity.Property(e => e.PasswordHash).HasMaxLength(500);

            entity.HasOne(e => e.EmployeeNavigation)
                .WithOne(e => e.UserNavigation)
                .HasForeignKey<User>(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.RoleNavigation)
                .WithMany(e => e.Users)
                .HasForeignKey(e => e.Role)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasIndex(e => e.EmployeeId, "IX_ActivityLogs_EmployeeId");

            entity.HasIndex(e => e.UserId, "IX_ActivityLogs_UserId");

            entity.HasOne(d => d.Employee).WithMany(p => p.ActivityLogs).HasForeignKey(d => d.EmployeeId);

            entity.HasOne(d => d.User).WithMany(p => p.ActivityLogs).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<ContactPerson>(entity =>
        {
            entity.HasIndex(e => e.CustomerId, "IX_ContactPersons_CustomerId");

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Position).HasMaxLength(100);

            entity.HasOne(d => d.Customer).WithMany(p => p.ContactPeople).HasForeignKey(d => d.CustomerId);

            entity.HasMany(d => d.OrderPoints)
                .WithOne(p => p.ContactPersonNavigation)
                .HasForeignKey(p => p.ContactPersonId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.Edrpou).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.Phone).HasMaxLength(30);
        });

        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Cargos_VolumeM3_NonNegative", "\"VolumeM3\" IS NULL OR \"VolumeM3\" >= 0");
                table.HasCheckConstraint("CK_Cargos_TemperatureRange", "\"MinTemperatureC\" IS NULL OR \"MaxTemperatureC\" IS NULL OR \"MinTemperatureC\" <= \"MaxTemperatureC\"");
            });
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.OrderStatusId);

            entity.HasOne(e => e.CustomerNavigation)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.OrderStatusNavigation)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.OrderStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DriverReport>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt, "IX_DriverReports_CreatedAt");

            entity.HasIndex(e => e.DriverId, "IX_DriverReports_DriverId");

            entity.HasIndex(e => e.OrderId, "IX_DriverReports_OrderId");

            entity.HasIndex(e => e.RouteId, "IX_DriverReports_RouteId");

            entity.Property(e => e.CargoCondition).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ReportType).HasMaxLength(100);
            entity.Property(e => e.VehicleCondition).HasMaxLength(500);

            entity.HasOne(d => d.Driver).WithMany(p => p.DriverReports)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Order).WithMany(p => p.DriverReports)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Route).WithMany(p => p.DriverReports)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DriverSchedule>(entity =>
        {
            entity.HasIndex(e => e.EmployeeId, "IX_DriverSchedules_EmployeeId");

            entity.HasOne(d => d.Employee).WithMany(p => p.DriverSchedules).HasForeignKey(d => d.EmployeeId);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_Employees_RoleId");

            entity.Property(e => e.Address).HasDefaultValueSql("''::text");
            entity.Property(e => e.Email).HasDefaultValueSql("''::text");
            entity.Property(e => e.PhotoPath).HasDefaultValueSql("''::text");
            entity.Property(e => e.RoleId).HasDefaultValue(0);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.TaxId).HasMaxLength(10);
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_Employees_TaxId_Format",
                "\"TaxId\" IS NULL OR \"TaxId\" ~ '^[0-9]{10}$'"));

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmployeeDocument>(entity =>
        {
            entity.HasIndex(e => e.EmployeeId, "IX_EmployeeDocuments_EmployeeId");

            entity.Property(e => e.DocumentNumber).HasMaxLength(100);
            entity.Property(e => e.IssuedBy).HasMaxLength(200);
            entity.Property(e => e.DocumentSeries).HasMaxLength(20);
            entity.Property(e => e.UniqueRecordNumber).HasMaxLength(20);
            entity.Property(e => e.InstitutionName).HasMaxLength(200);
            entity.Property(e => e.Specialty).HasMaxLength(200);
            entity.Property(e => e.MilitarySpecialtyCode).HasMaxLength(10);
            entity.Property(e => e.MilitaryRank).HasMaxLength(100);
            entity.Property(e => e.RecruitmentOffice).HasMaxLength(200);
            entity.Property(e => e.FilePath).HasMaxLength(500);

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeDocuments).HasForeignKey(d => d.EmployeeId);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt, "IX_Notifications_CreatedAt");

            entity.HasIndex(e => e.EmployeeId, "IX_Notifications_EmployeeId");

            entity.HasIndex(e => e.IsRead, "IX_Notifications_IsRead");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(2000);
            entity.Property(e => e.NotificationType).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Employee).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderAssignment>(entity =>
        {
            entity.HasIndex(e => new { e.DriverId, e.PlannedStart, e.PlannedEnd }, "IX_OrderAssignments_DriverId_PlannedStart_PlannedEnd");

            entity.HasIndex(e => e.OrderId, "IX_OrderAssignments_OrderId");

            entity.HasIndex(e => new { e.VehicleId, e.PlannedStart, e.PlannedEnd }, "IX_OrderAssignments_VehicleId_PlannedStart_PlannedEnd");

            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Driver).WithMany(p => p.OrderAssignments)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderAssignments).HasForeignKey(d => d.OrderId);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.OrderAssignments)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderCargo>(entity =>
        {
            entity.HasIndex(e => e.CargoId, "IX_OrderCargos_CargoId");

            entity.HasIndex(e => new { e.OrderId, e.CargoId }, "IX_OrderCargos_OrderId_CargoId").IsUnique();

            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(d => d.Cargo).WithMany(p => p.OrderCargos)
                .HasForeignKey(d => d.CargoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderCargos).HasForeignKey(d => d.OrderId);
        });

        modelBuilder.Entity<OrderPoint>(entity =>
        {
            entity.HasIndex(e => new { e.OrderId, e.SequenceNumber }, "IX_OrderPoints_OrderId_SequenceNumber").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.ContactName).HasMaxLength(150);
            entity.Property(e => e.ContactPhone).HasMaxLength(30);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PointType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderPoints).HasForeignKey(d => d.OrderId);

            entity.HasIndex(e => e.ContactPersonId);
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasIndex(e => e.Name, "IX_OrderStatuses_Name").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(e => e.VehicleStatusId);

            entity.HasOne(e => e.VehicleStatusNavigation)
                .WithMany(e => e.Vehicles)
                .HasForeignKey(e => e.VehicleStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Vehicles_Dimensions_NonNegative", "(\"ExteriorLengthMeters\" IS NULL OR \"ExteriorLengthMeters\" >= 0) AND (\"ExteriorWidthMeters\" IS NULL OR \"ExteriorWidthMeters\" >= 0) AND (\"ExteriorHeightMeters\" IS NULL OR \"ExteriorHeightMeters\" >= 0) AND (\"InteriorLengthMeters\" IS NULL OR \"InteriorLengthMeters\" >= 0) AND (\"InteriorWidthMeters\" IS NULL OR \"InteriorWidthMeters\" >= 0) AND (\"InteriorHeightMeters\" IS NULL OR \"InteriorHeightMeters\" >= 0) AND (\"EmptyWeightKg\" IS NULL OR \"EmptyWeightKg\" >= 0) AND (\"MaxGrossWeightKg\" IS NULL OR \"MaxGrossWeightKg\" >= 0)");
            });
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasIndex(e => e.ChangedAt, "IX_OrderStatusHistories_ChangedAt");

            entity.HasIndex(e => e.ChangedByEmployeeId, "IX_OrderStatusHistories_ChangedByEmployeeId");

            entity.HasIndex(e => e.OrderId, "IX_OrderStatusHistories_OrderId");

            entity.Property(e => e.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.NewStatus).HasMaxLength(100);
            entity.Property(e => e.PreviousStatus).HasMaxLength(100);

            entity.HasOne(d => d.ChangedByEmployee).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.ChangedByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderStatusHistories).HasForeignKey(d => d.OrderId);
        });

        modelBuilder.Entity<RoadEvent>(entity =>
        {
            entity.HasIndex(e => e.EventType, "IX_RoadEvents_EventType");

            entity.HasIndex(e => e.IsActive, "IX_RoadEvents_IsActive");

            entity.HasIndex(e => e.StartedAt, "IX_RoadEvents_StartedAt");

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Severity).HasMaxLength(50);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<RoadRestriction>(entity =>
        {
            entity.HasIndex(e => e.IsActive, "IX_RoadRestrictions_IsActive");

            entity.HasIndex(e => e.RestrictionType, "IX_RoadRestrictions_RestrictionType");

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RestrictionType).HasMaxLength(100);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<TransportIn.Data.Entities.Route>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_Routes_OrderId");

            entity.HasIndex(e => e.VehicleId, "IX_Routes_VehicleId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.EndPoint).HasMaxLength(300);
            entity.Property(e => e.StartPoint).HasMaxLength(300);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Order).WithMany(p => p.Routes).HasForeignKey(d => d.OrderId);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Routes)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RouteHistory>(entity =>
        {
            entity.HasIndex(e => e.ChangedAt, "IX_RouteHistories_ChangedAt");

            entity.HasIndex(e => e.ChangedByEmployeeId, "IX_RouteHistories_ChangedByEmployeeId");

            entity.HasIndex(e => e.RouteId, "IX_RouteHistories_RouteId");

            entity.Property(e => e.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.NewStatus).HasMaxLength(100);
            entity.Property(e => e.PreviousStatus).HasMaxLength(100);

            entity.HasOne(d => d.ChangedByEmployee).WithMany(p => p.RouteHistories)
                .HasForeignKey(d => d.ChangedByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Route).WithMany(p => p.RouteHistories).HasForeignKey(d => d.RouteId);
        });

        modelBuilder.Entity<RoutePoint>(entity =>
        {
            entity.HasIndex(e => new { e.RouteId, e.SequenceNumber }, "IX_RoutePoints_RouteId_SequenceNumber").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PointType).HasMaxLength(50);

            entity.HasOne(d => d.Route).WithMany(p => p.RoutePoints).HasForeignKey(d => d.RouteId);
        });

        modelBuilder.Entity<RuleCondition>(entity =>
        {
            entity.HasIndex(e => new { e.ExpertRuleId, e.SequenceNumber }, "IX_RuleConditions_ExpertRuleId_SequenceNumber").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ExpectedValue).HasMaxLength(200);
            entity.Property(e => e.LogicalOperator).HasMaxLength(10);
            entity.Property(e => e.Operator).HasMaxLength(30);
            entity.Property(e => e.Parameter).HasMaxLength(150);

            entity.HasOne(d => d.ExpertRule).WithMany(p => p.RuleConditions).HasForeignKey(d => d.ExpertRuleId);
        });

        modelBuilder.Entity<RuleExecutionHistory>(entity =>
        {
            entity.HasIndex(e => e.EmployeeId, "IX_RuleExecutionHistories_EmployeeId");

            entity.HasIndex(e => e.ExecutedAt, "IX_RuleExecutionHistories_ExecutedAt");

            entity.HasIndex(e => e.ExpertRuleId, "IX_RuleExecutionHistories_ExpertRuleId");

            entity.HasIndex(e => e.WasSuccessful, "IX_RuleExecutionHistories_WasSuccessful");

            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.ExecutedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Result).HasMaxLength(300);
            entity.Property(e => e.TriggerDescription).HasMaxLength(500);

            entity.HasOne(d => d.Employee).WithMany(p => p.RuleExecutionHistories)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.ExpertRule).WithMany(p => p.RuleExecutionHistories).HasForeignKey(d => d.ExpertRuleId);
        });

        modelBuilder.Entity<SafeLocation>(entity =>
        {
            entity.HasIndex(e => e.IsActive, "IX_SafeLocations_IsActive");

            entity.HasIndex(e => e.Name, "IX_SafeLocations_Name");

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LocationType).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<VehicleDocument>(entity =>
        {
            entity.HasIndex(e => e.VehicleId, "IX_VehicleDocuments_VehicleId");

            entity.Property(e => e.DocumentNumber).HasMaxLength(100);
            entity.Property(e => e.IssuedBy).HasMaxLength(200);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleDocuments).HasForeignKey(d => d.VehicleId);
        });

        modelBuilder.Entity<VehicleEquipment>(entity =>
        {
            entity.HasIndex(e => e.VehicleId, "IX_VehicleEquipments_VehicleId");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleEquipments).HasForeignKey(d => d.VehicleId);
        });

        modelBuilder.Entity<VehicleLocation>(entity =>
        {
            entity.HasIndex(e => new { e.VehicleId, e.RecordedAt }, "IX_VehicleLocations_VehicleId_RecordedAt");

            entity.Property(e => e.RecordedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleLocations).HasForeignKey(d => d.VehicleId);
        });

        modelBuilder.Entity<VehicleStatus>(entity =>
        {
            entity.HasIndex(e => e.Name, "IX_VehicleStatuses_Name").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
