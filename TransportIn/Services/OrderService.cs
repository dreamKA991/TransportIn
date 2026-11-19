
using Microsoft.EntityFrameworkCore;
using TransportIn.Data;
using TransportIn.Models;
using OrderEntity = TransportIn.Data.Entities.Order;
using OrderModel = TransportIn.Models.Order;

namespace TransportIn.Services
{
    public class OrderDraft
    {
        public int Id { get; set; }

        public string CustomerCompany { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string CustomerEmail { get; set; } = "";

        public string Status { get; set; } = "Нове";
        public string AssignedVehiclePlate { get; set; } = "";

        public List<CargoDraft> Cargos { get; set; } = new();
        public List<PointDraft> Points { get; set; } = new();
    }

    public class CargoDraft
    {
        public string Name { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public double WeightTons { get; set; }
        public double VolumeM3 { get; set; }
        public bool NeedsRefrigerator { get; set; }
        public double? TempMin { get; set; }
        public double? TempMax { get; set; }
        public string SpecialConditions { get; set; } = "";
    }

    public class PointDraft
    {
        public bool IsLoading { get; set; }
        public string Address { get; set; } = "";
        public string ContactName { get; set; } = "";
        public string ContactPhone { get; set; } = "";
    }

    public class OrderService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public OrderService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public event Action? OnChange;

        public List<OrderModel> GetAllOrders()
        {
            using var db = _contextFactory.CreateDbContext();

            var query = db.Orders
                .AsNoTracking()
                .AsSplitQuery()
                .Include(order => order.OrderPoints)
                .Include(order => order.OrderCargos)
                    .ThenInclude(orderCargo => orderCargo.Cargo)
                .Include(order => order.OrderAssignments)
                    .ThenInclude(assignment => assignment.Vehicle)
                .OrderBy(order => order.Id);

            Console.WriteLine("ПОЛНЫЙ SQL ЗАПРОС:");
            Console.WriteLine(query.ToQueryString());

            var orders = query.ToList();

            return orders.Select(order =>
            {
                var points = order.OrderPoints
                    .OrderBy(point => point.SequenceNumber)
                    .ToList();

                var cargoLinks = order.OrderCargos.ToList();

                var loadingPoints = points
                    .Where(point => IsLoadingPoint(point.PointType))
                    .Select(point => point.Address)
                    .Where(address => !string.IsNullOrWhiteSpace(address))
                    .ToList();

                var deliveryPoints = points
                    .Where(point => !IsLoadingPoint(point.PointType))
                    .Select(point => point.Address)
                    .Where(address => !string.IsNullOrWhiteSpace(address))
                    .ToList();

                var cargoName = cargoLinks.Count == 0
                    ? order.CargoName
                    : string.Join(", ", cargoLinks.Select(link => link.Cargo.Title));

                var cargoWeightTons = cargoLinks.Sum(
                    link => link.Cargo.WeightKg * link.Quantity) / 1000d;

                var cargoRequiresRefrigerator = cargoLinks.Any(link =>
                    link.Cargo.TransportConditions.Contains("реф", StringComparison.OrdinalIgnoreCase) ||
                    link.Cargo.TransportConditions.Contains("холод", StringComparison.OrdinalIgnoreCase) ||
                    link.Cargo.TransportConditions.Contains("refriger", StringComparison.OrdinalIgnoreCase));

                var latestAssignment = order.OrderAssignments
                    .OrderByDescending(assignment => assignment.AssignedAt)
                    .FirstOrDefault();

                return new OrderModel
                {
                    Id = order.Id,
                    CustomerCompany = order.CustomerCompany,
                    CustomerName = order.CustomerName,
                    CustomerPhone = order.CustomerPhone,
                    CustomerEmail = order.CustomerEmail,
                    CargoName = cargoName,
                    WeightTons = cargoWeightTons > 0
                        ? cargoWeightTons
                        : order.WeightTons,
                    VolumeM3 = order.VolumeM3,
                    RequiresRefrigerator = cargoLinks.Count > 0
                        ? cargoRequiresRefrigerator
                        : order.RequiresRefrigerator,
                    LoadingPoints = loadingPoints.Count > 0
                        ? loadingPoints
                        : SplitPoints(order.LoadingPoints),
                    DeliveryPoints = deliveryPoints.Count > 0
                        ? deliveryPoints
                        : SplitPoints(order.DeliveryPoints),
                    AssignedVehiclePlate = latestAssignment?.Vehicle.LicensePlate
                        ?? order.AssignedVehiclePlate,
                    Status = order.Status
                };
            }).ToList();
        }

        public void AddOrder(OrderModel order)
        {
            using var db = _contextFactory.CreateDbContext();

            var entity = new OrderEntity
            {
                CustomerCompany = order.CustomerCompany,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                CustomerEmail = order.CustomerEmail,
                CargoName = order.CargoName,
                WeightTons = order.WeightTons,
                VolumeM3 = order.VolumeM3,
                RequiresRefrigerator = order.RequiresRefrigerator,
                LoadingPoints = JoinPoints(order.LoadingPoints),
                DeliveryPoints = JoinPoints(order.DeliveryPoints),
                AssignedVehiclePlate = order.AssignedVehiclePlate,
                Status = order.Status
            };

            db.Orders.Add(entity);
            db.SaveChanges();

            order.Id = entity.Id;
            NotifyStateChanged();
        }

        public List<Vehicle> GetSuitableVehicles(
            OrderModel order,
            List<Vehicle> allVehicles)
        {
            return allVehicles.Where(vehicle =>
                vehicle.CapacityTons >= order.WeightTons &&
                vehicle.VolumeM3 >= order.VolumeM3 &&
                (!order.RequiresRefrigerator || vehicle.HasRefrigerator)
            ).ToList();
        }

        public void AssignVehicle(int orderId, string licensePlate)
        {
            using var db = _contextFactory.CreateDbContext();

            var order = db.Orders.FirstOrDefault(item => item.Id == orderId);
            if (order == null)
                return;

            order.AssignedVehiclePlate = licensePlate;
            order.Status = "Призначено транспорт";

            db.SaveChanges();
            NotifyStateChanged();
        }

        public void UpdateStatus(int orderId, string status)
        {
            using var db = _contextFactory.CreateDbContext();

            var order = db.Orders.FirstOrDefault(item => item.Id == orderId);
            if (order == null)
                return;

            order.Status = status;

            db.SaveChanges();
            NotifyStateChanged();
        }

        private static bool IsLoadingPoint(string pointType) =>
            pointType.Contains("load", StringComparison.OrdinalIgnoreCase) ||
            pointType.Contains("pickup", StringComparison.OrdinalIgnoreCase) ||
            pointType.Contains("завантаж", StringComparison.OrdinalIgnoreCase) ||
            pointType.Contains("відправ", StringComparison.OrdinalIgnoreCase);

        private static List<string> SplitPoints(string? value) =>
            string.IsNullOrWhiteSpace(value)
                ? new List<string>()
                : value.Split(
                    new[] { '|', ';', '\r', '\n' },
                    StringSplitOptions.TrimEntries |
                    StringSplitOptions.RemoveEmptyEntries
                ).ToList();

        private static string JoinPoints(IEnumerable<string> points) =>
            string.Join(
                " | ",
                points.Where(point => !string.IsNullOrWhiteSpace(point))
                    .Select(point => point.Trim())
            );

        public void AddOrder(OrderDraft draft)
        {
            using var db = _contextFactory.CreateDbContext();
            using var transaction = db.Database.BeginTransaction();

            try
            {
                var order = new OrderEntity
                {
                    CustomerCompany = draft.CustomerCompany,
                    CustomerName = draft.CustomerName,
                    CustomerPhone = draft.CustomerPhone,
                    CustomerEmail = draft.CustomerEmail,

                    CargoName = string.Join(", ",
                        draft.Cargos.Select(c => c.Name)),

                    WeightTons = draft.Cargos.Sum(
                        c => c.WeightTons * c.Quantity),

                    VolumeM3 = draft.Cargos.Sum(
                        c => c.VolumeM3 * c.Quantity),

                    RequiresRefrigerator =
                        draft.Cargos.Any(c => c.NeedsRefrigerator),

                    LoadingPoints = JoinPoints(
                        draft.Points.Where(p => p.IsLoading)
                            .Select(p => p.Address)),

                    DeliveryPoints = JoinPoints(
                        draft.Points.Where(p => !p.IsLoading)
                            .Select(p => p.Address)),

                    AssignedVehiclePlate = draft.AssignedVehiclePlate,
                    Status = draft.Status
                };

                db.Orders.Add(order);
                db.SaveChanges();

                SaveCargos(db, order.Id, draft.Cargos);
                SavePoints(db, order.Id, draft.Points);

                transaction.Commit();

                draft.Id = order.Id;
                NotifyStateChanged();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void UpdateOrder(OrderDraft draft)
        {
            using var db = _contextFactory.CreateDbContext();
            using var transaction = db.Database.BeginTransaction();

            try
            {
                var order = db.Orders
                    .Include(o => o.OrderCargos)
                        .ThenInclude(oc => oc.Cargo)
                    .Include(o => o.OrderPoints)
                    .FirstOrDefault(o => o.Id == draft.Id);

                if (order == null)
                    throw new InvalidOperationException("Замовлення не знайдено.");

                order.CustomerCompany = draft.CustomerCompany;
                order.CustomerName = draft.CustomerName;
                order.CustomerPhone = draft.CustomerPhone;
                order.CustomerEmail = draft.CustomerEmail;

                order.CargoName = string.Join(", ",
                    draft.Cargos.Select(c => c.Name));

                order.WeightTons = draft.Cargos.Sum(
                    c => c.WeightTons * c.Quantity);

                order.VolumeM3 = draft.Cargos.Sum(
                    c => c.VolumeM3 * c.Quantity);

                order.RequiresRefrigerator =
                    draft.Cargos.Any(c => c.NeedsRefrigerator);

                order.LoadingPoints = JoinPoints(
                    draft.Points.Where(p => p.IsLoading)
                        .Select(p => p.Address));

                order.DeliveryPoints = JoinPoints(
                    draft.Points.Where(p => !p.IsLoading)
                        .Select(p => p.Address));

                order.AssignedVehiclePlate = draft.AssignedVehiclePlate;
                order.Status = draft.Status;

                var oldCargos = order.OrderCargos
                    .Select(oc => oc.Cargo)
                    .ToList();

                db.OrderCargos.RemoveRange(order.OrderCargos);
                db.OrderPoints.RemoveRange(order.OrderPoints);

                db.SaveChanges();

                foreach (var cargo in oldCargos)
                {
                    if (!db.OrderCargos.Any(oc => oc.CargoId == cargo.Id))
                        db.Cargos.Remove(cargo);
                }

                db.SaveChanges();

                SaveCargos(db, order.Id, draft.Cargos);
                SavePoints(db, order.Id, draft.Points);

                transaction.Commit();
                NotifyStateChanged();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private void SaveCargos(
            AppDbContext db,
            int orderId,
            List<CargoDraft> cargos)
        {
            foreach (var item in cargos)
            {
                var cargo = new TransportIn.Data.Entities.Cargo
                {
                    Title = item.Name,
                    WeightKg = item.WeightTons * 1000,
                    VolumeM3 = item.VolumeM3,

                    NeedsRefrigerator = item.NeedsRefrigerator,
                    MinTemperatureC = item.TempMin,
                    MaxTemperatureC = item.TempMax,

                    SpecialConditions = item.SpecialConditions,
                    TransportConditions = item.NeedsRefrigerator
                        ? "Рефрижератор"
                        : item.SpecialConditions,

                    SenderName = "",
                    SenderPhone = "",
                    RecipientName = "",
                    RecipientPhone = ""
                };

                db.Cargos.Add(cargo);
                db.SaveChanges();

                db.OrderCargos.Add(
                    new TransportIn.Data.Entities.OrderCargo
                    {
                        OrderId = orderId,
                        CargoId = cargo.Id,
                        Quantity = item.Quantity,
                        Notes = item.SpecialConditions
                    });
            }

            db.SaveChanges();
        }

        private void SavePoints(
            AppDbContext db,
            int orderId,
            List<PointDraft> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                var item = points[i];

                db.OrderPoints.Add(
                    new TransportIn.Data.Entities.OrderPoint
                    {
                        OrderId = orderId,
                        ContactPersonId = null,

                        PointType = item.IsLoading
                            ? "Завантаження"
                            : "Доставка",

                        Address = item.Address,
                        ContactName = item.ContactName,
                        ContactPhone = item.ContactPhone,

                        SequenceNumber = i + 1,

                        Latitude = null,
                        Longitude = null,
                        PlannedArrival = null,
                        ActualArrival = null,

                        Status = "Нова",
                        Notes = ""
                    });
            }

            db.SaveChanges();
        }

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }
    }
}
