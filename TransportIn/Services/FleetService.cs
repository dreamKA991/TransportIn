
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransportIn.Data;
using TransportIn.Models;

namespace TransportIn.Services
{
    public class FleetService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public FleetService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public event Action? OnChange;

        public List<Vehicle> GetAllVehicles()
        {
            using var context = _contextFactory.CreateDbContext();

            return context.Vehicles
                .AsNoTracking()
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

        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            return await context.Vehicles
                .AsNoTracking()
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
                .ToListAsync();
        }

        public void AddVehicle(Vehicle vehicle)
        {
            using var context = _contextFactory.CreateDbContext();

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

            context.Vehicles.Add(dbVehicle);
            context.SaveChanges();

            vehicle.Id = dbVehicle.Id;

            NotifyStateChanged();
        }

        public void DeleteVehicle(int id)
        {
            using var context = _contextFactory.CreateDbContext();

            var vehicle = context.Vehicles.FirstOrDefault(v => v.Id == id);

            if (vehicle != null)
            {
                context.Vehicles.Remove(vehicle);
                context.SaveChanges();

                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }
    }
}
