using System;
using System.Collections.Generic;

namespace TransportIn.Data.Entities;

public partial class Order
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public int? OrderStatusId { get; set; }

    public string CustomerCompany { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string CustomerEmail { get; set; } = null!;

    public string CargoName { get; set; } = null!;

    public double WeightTons { get; set; }

    public double VolumeM3 { get; set; }

    public bool RequiresRefrigerator { get; set; }

    public string LoadingPoints { get; set; } = null!;

    public string DeliveryPoints { get; set; } = null!;

    public string AssignedVehiclePlate { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual Customer? CustomerNavigation { get; set; }

    public virtual OrderStatus? OrderStatusNavigation { get; set; }

    public virtual ICollection<DriverReport> DriverReports { get; set; } = new List<DriverReport>();

    public virtual ICollection<OrderAssignment> OrderAssignments { get; set; } = new List<OrderAssignment>();

    public virtual ICollection<OrderCargo> OrderCargos { get; set; } = new List<OrderCargo>();

    public virtual ICollection<OrderPoint> OrderPoints { get; set; } = new List<OrderPoint>();

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual ICollection<Route> Routes { get; set; } = new List<Route>();
}
