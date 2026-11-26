using TransportIn.Services;

namespace TransportIn.Tests;

public class OrderServiceTests
{
    [Fact]
    public void GetActiveOrderForDriver_ReturnsNullWhenEmployeeIdIsMissing()
    {
        var orders = new OrderService(new NoDatabaseContextFactory());

        Assert.Null(orders.GetActiveOrderForDriver(employeeId: null));
    }

    [Fact]
    public void AssignVehiclesToOrder_RejectsEmptySelection()
    {
        var orders = new OrderService(new NoDatabaseContextFactory());

        var error = orders.AssignVehiclesToOrder(orderId: 1, licensePlates: []);

        Assert.Equal("Оберіть хоча б один транспортний засіб.", error);
    }

    [Fact]
    public void AssignVehiclesToOrder_RejectsBlankLicensePlates()
    {
        var orders = new OrderService(new NoDatabaseContextFactory());

        var error = orders.AssignVehiclesToOrder(orderId: 1, licensePlates: ["", "  ", "\t"]);

        Assert.Equal("Оберіть хоча б один транспортний засіб.", error);
    }
}
