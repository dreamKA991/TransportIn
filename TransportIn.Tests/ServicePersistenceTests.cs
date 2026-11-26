using Microsoft.EntityFrameworkCore;
using TransportIn.Data.Entities;
using TransportIn.Services;

namespace TransportIn.Tests.Persistence;

[Collection("PostgreSQL")]
public class ServicePersistenceTests(PostgresDatabase database)
{
    [PostgresFact]
    [Trait("Suite", "Persistence")]
    public void MultiVehicleOrder_SavesAssignmentsAndCompletedDriverStages()
    {
        var order = OrderPersistenceTests.CreateOrder(database, "QA Перевезення на двох ТЗ");
        var fleet = new FleetService(database);
        fleet.AddVehicle(new TransportIn.Models.Vehicle
        {
            BrandModel = "QA Один", LicensePlate = "AA 7350 BP", VinCode = "WVWZZZ1JZXW000151",
            CapacityTons = 2, VolumeM3 = 5
        });
        fleet.AddVehicle(new TransportIn.Models.Vehicle
        {
            BrandModel = "QA Два", LicensePlate = "AA 7351 BP", VinCode = "WVWZZZ1JZXW000152",
            CapacityTons = 2, VolumeM3 = 5
        });
        var first = CreateDriver("Водій Один QA", "0500000930");
        var second = CreateDriver("Водій Два QA", "0500000931");
        var orders = new OrderService(database);

        Assert.NotNull(orders.AssignVehiclesToOrder(order.Id, ["AA 7350 BP"]));
        Assert.Null(orders.AssignVehiclesToOrder(order.Id, ["AA 7350 BP", "AA 7351 BP"]));
        Assert.Null(orders.AssignDriverToVehicle(order.Id, "AA 7350 BP", first.FullName));
        Assert.Null(orders.AssignDriverToVehicle(order.Id, "AA 7351 BP", second.FullName));
        Assert.Equal("AA 7350 BP", orders.GetActiveOrderForDriver(first.Id)?.AssignedVehiclePlate);
        Assert.Equal("AA 7351 BP", orders.GetActiveOrderForDriver(second.Id)?.AssignedVehiclePlate);

        var points = orders.GetPointProgress(order.Id);
        Assert.False(orders.CompleteOrder(order.Id));
        Assert.False(orders.SetPointStatus(points[1].Id, "Прибув"));
        foreach (var status in new[] { "Прибув", "Завантажується", "Завантажено", "Виїхав" })
            Assert.True(orders.SetPointStatus(points[0].Id, status));
        foreach (var status in new[] { "Прибув", "Розвантажується", "Розвантажено" })
            Assert.True(orders.SetPointStatus(points[1].Id, status));
        Assert.True(orders.CompleteOrder(order.Id));

        using var db = database.CreateDbContext();
        var saved = db.Orders.Include(o => o.OrderAssignments).ThenInclude(a => a.Vehicle)
            .Include(o => o.OrderPoints).Single(o => o.Id == order.Id);
        Assert.Equal("Завершено", saved.Status);
        Assert.Equal("Розвантажено", saved.OrderPoints.Single(p => p.SequenceNumber == 2).Status);
        Assert.Equal(new[] { (first.Id, "AA 7350 BP"), (second.Id, "AA 7351 BP") },
            saved.OrderAssignments.OrderBy(a => a.Vehicle.LicensePlate)
                .Select(a => (a.DriverId, a.Vehicle.LicensePlate)));
        Assert.Contains(db.OrderStatusHistories, h => h.OrderId == order.Id && h.NewStatus == "Завершено");
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    public void OrderCreation_RollsBackOrderWhenCargoCannotBeSaved()
    {
        const string company = "QA Відкат транзакції";
        var draft = new OrderDraft
        {
            CustomerCompany = company, CustomerName = "QA Контакт", CustomerPhone = "0500000914",
            CustomerEmail = "order@example.test", Status = "Нове",
            Cargos = [new() { Name = null!, Quantity = 1, WeightTons = 1, VolumeM3 = 2 }]
        };

        Assert.Throws<DbUpdateException>(() => new OrderService(database).AddOrder(draft));
        using var db = database.CreateDbContext();
        Assert.False(db.Orders.Any(o => o.CustomerCompany == company));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public async Task RoadEventService_CanCreateEventWithoutSuppliedTimestamp()
    {
        var service = new RoadEventService(database);
        var roadEvent = new RoadEvent
        {
            EventType = "Ремонт дороги", Title = "QA Дата початку", Description = "QA Перевірка",
            Address = "QA Вулиця 104", Severity = "Середня", IsActive = true
        };
        await service.AddAsync(roadEvent);

        await using var db = database.CreateDbContext();
        var saved = await db.RoadEvents.SingleAsync(e => e.Id == roadEvent.Id);
        Assert.Equal("QA Вулиця 104", saved.Address);
        Assert.NotEqual(default, saved.StartedAt);
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public async Task RoadEventService_CanCloseAnActiveEvent()
    {
        var service = new RoadEventService(database);
        var roadEvent = new RoadEvent
        {
            EventType = "Ремонт дороги", Title = "QA Дата завершення", Description = "QA Перевірка",
            Address = "QA Вулиця 105", Severity = "Середня", IsActive = true, StartedAt = DateTime.UtcNow
        };
        await service.AddAsync(roadEvent);
        await service.SetActiveAsync(roadEvent.Id, false);

        await using var db = database.CreateDbContext();
        var saved = await db.RoadEvents.SingleAsync(e => e.Id == roadEvent.Id);
        Assert.False(saved.IsActive);
        Assert.NotNull(saved.EndedAt);
    }

    private TransportIn.Services.Employee CreateDriver(string name, string phone)
    {
        var driver = new TransportIn.Services.Employee
        {
            FullName = name, Phone = phone, Role = "Водій",
            LicenseCategory = "C", MedicalCertExpiry = "01.12.2030"
        };
        new EmployeeService(database).AddEmployee(driver);
        return driver;
    }
}
