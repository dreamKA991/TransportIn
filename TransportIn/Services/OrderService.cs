using Microsoft.EntityFrameworkCore;
using TransportIn.Data;
using TransportIn.Models;
using OrderEntity = TransportIn.Data.Entities.Order;
using OrderModel = TransportIn.Models.Order;

namespace TransportIn.Services
{
    public class OrderService
    {
        private readonly AppDbContext _db;

        public OrderService(AppDbContext db) => _db = db;

        public event Action? OnChange;

        public List<OrderModel> GetAllOrders()
        {
            var orders = _db.Orders
                .AsNoTracking()
                .Include(order => order.OrderPoints)
                .Include(order => order.OrderCargos)
                    .ThenInclude(orderCargo => orderCargo.Cargo)
                .Include(order => order.OrderAssignments)
                    .ThenInclude(assignment => assignment.Vehicle)
                .OrderBy(order => order.Id)
                .ToList();

            return orders.Select(order =>
            {
                var points = order.OrderPoints.OrderBy(point => point.SequenceNumber).ToList();
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
                var cargoWeightTons = cargoLinks.Sum(link => link.Cargo.WeightKg * link.Quantity) / 1000d;
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
                    WeightTons = cargoWeightTons > 0 ? cargoWeightTons : order.WeightTons,
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

            _db.Orders.Add(entity);
            _db.SaveChanges();
            order.Id = entity.Id;
            NotifyStateChanged();
        }

        public List<Vehicle> GetSuitableVehicles(OrderModel order, List<Vehicle> allVehicles)
        {
            return allVehicles.Where(vehicle =>
                vehicle.CapacityTons >= order.WeightTons &&
                vehicle.VolumeM3 >= order.VolumeM3 &&
                (!order.RequiresRefrigerator || vehicle.HasRefrigerator)
            ).ToList();
        }

        public void AssignVehicle(int orderId, string licensePlate)
        {
            var order = _db.Orders.FirstOrDefault(item => item.Id == orderId);
            if (order == null) return;

            order.AssignedVehiclePlate = licensePlate;
            order.Status = "Призначено транспорт";
            _db.SaveChanges();
            NotifyStateChanged();
        }

        public void UpdateStatus(int orderId, string status)
        {
            var order = _db.Orders.FirstOrDefault(item => item.Id == orderId);
            if (order == null) return;

            order.Status = status;
            _db.SaveChanges();
            NotifyStateChanged();
        }

        private static bool IsLoadingPoint(string pointType) =>
            pointType.Contains("load", StringComparison.OrdinalIgnoreCase) ||
            pointType.Contains("pickup", StringComparison.OrdinalIgnoreCase) ||
            pointType.Contains("завантаж", StringComparison.OrdinalIgnoreCase) ||
            pointType.Contains("відправ", StringComparison.OrdinalIgnoreCase);

        private static List<string> SplitPoints(string? value) => string.IsNullOrWhiteSpace(value)
            ? new List<string>()
            : value.Split(new[] { '|', ';', '\r', '\n' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

        private static string JoinPoints(IEnumerable<string> points) =>
            string.Join(" | ", points.Where(point => !string.IsNullOrWhiteSpace(point)).Select(point => point.Trim()));

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
