
using TransportIn.Data;
using TransportIn.Models;
using System.Linq;

namespace TransportIn.Services
{
    public class FleetService
    {
        private readonly AppDbContext _context;

        public FleetService(AppDbContext context)
        {
            _context = context;
        }

        public event Action? OnChange;

        // Получить все автомобили из PostgreSQL
        public List<Vehicle> GetAllVehicles()
        {
            return _context.Vehicles
                .Select(v => new Vehicle
                {
                    Id = v.Id,
                    BrandModel = v.BrandModel,
                    LicensePlate = v.LicensePlate,
                    VinCode = v.VinCode,
                    BodyType = v.BodyType,
                    CapacityTons = v.CapacityTons,
                    VolumeM3 = v.VolumeM3,
                    HasRefrigerator = v.HasRefrigerator,
                    AssignedDriver = v.AssignedDriver,
                    Status = v.Status,
                    Lat = v.Lat,
                    Lng = v.Lng
                })
                .ToList();
        }

        // Добавить автомобиль в PostgreSQL
        public void AddVehicle(Vehicle vehicle)
        {
            var dbVehicle = new TransportIn.Data.Entities.Vehicle
            {
                BrandModel = vehicle.BrandModel,
                LicensePlate = vehicle.LicensePlate,
                VinCode = vehicle.VinCode,
                BodyType = vehicle.BodyType,
                CapacityTons = vehicle.CapacityTons,
                VolumeM3 = vehicle.VolumeM3,
                HasRefrigerator = vehicle.HasRefrigerator,
                AssignedDriver = vehicle.AssignedDriver,
                Status = vehicle.Status,
                Lat = vehicle.Lat,
                Lng = vehicle.Lng
            };

            _context.Vehicles.Add(dbVehicle);
            _context.SaveChanges();

            vehicle.Id = dbVehicle.Id;

            NotifyStateChanged();
        }

        // Удалить автомобиль из PostgreSQL
        public void DeleteVehicle(int id)
        {
            var vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == id);

            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                _context.SaveChanges();

                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }
    }
}
