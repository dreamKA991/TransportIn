using Bunit;
using TransportIn.Components.Pages;
using TransportIn.Services;
using static TransportIn.Tests.Persistence.FormActions;

namespace TransportIn.Tests.Persistence;

[Collection("PostgreSQL")]
public class MessagePersistenceTests(PostgresDatabase database)
{
    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public void DriverReportForm_SavesTheReportOrNotifiesRecipient()
    {
        const string subject = "QA Проблема з вантажем";
        const string body = "QA Синтетичний опис проблеми з вантажем.";
        var draft = OrderPersistenceTests.CreateOrder(database, "QA Замовлення для повідомлення");
        new FleetService(database).AddVehicle(new TransportIn.Models.Vehicle
        {
            BrandModel = "QA для повідомлення", LicensePlate = "AA 7399 BP",
            VinCode = "WVWZZZ1JZXW000199", BodyType = "Тент", CapacityTons = 20, VolumeM3 = 80
        });
        var orders = new OrderService(database);
        Assert.Null(orders.AssignVehiclesToOrder(draft.Id, ["AA 7399 BP"]));
        Assert.Null(orders.AssignDriverToVehicle(draft.Id, "AA 7399 BP", PostgresDatabase.DriverName));

        using var ui = database.OpenSession(PostgresDatabase.DriverLogin);
        var form = ui.RenderComponent<DriverDashboard>();
        form.FindAll(".problem-type-btn").Single(button => button.TextContent.Contains("Клієнт / вантаж")).Click();
        form.Fill("Коротко опишіть проблему", subject);
        form.Fill("Детально опишіть ситуацію, місце, обставини...", body);
        form.ClickButton("Надіслати диспетчеру");
        Assert.Contains("успішно надіслано", form.Markup);

        using var db = database.CreateDbContext();
        var reportSaved = db.DriverReports.Any(report => report.OrderId == draft.Id &&
            report.Description.Contains(body));
        var notificationSaved = db.Notifications.Any(notification =>
            notification.Title == subject && notification.Message.Contains(body));
        CheckSavedFields(("повідомлення водія зі змістом", reportSaved || notificationSaved));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public void ManagerReplyForm_SavesTheReplyOutsideTheComponent()
    {
        const string reply = "QA Підтверджую отримання повідомлення.";
        using var ui = database.OpenSession("qa_fixture_manager");
        var form = ui.RenderComponent<ManagerDashboard>();
        form.FindAll(".msg-row")[0].Click();
        form.ClickButton("Відповісти");
        form.Field("Ваша відповідь...").Input(reply);
        form.ClickButton("Надіслати");
        Assert.Contains(reply, form.Markup);

        using var db = database.CreateDbContext();
        CheckSavedFields(("текст відповіді", db.Notifications.Any(notification => notification.Message.Contains(reply))));
    }
}
