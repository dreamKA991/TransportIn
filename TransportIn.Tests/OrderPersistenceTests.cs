using Bunit;
using Microsoft.EntityFrameworkCore;
using TransportIn.Components.Pages;
using TransportIn.Services;
using static TransportIn.Tests.Persistence.FormActions;

namespace TransportIn.Tests.Persistence;

[Collection("PostgreSQL")]
public class OrderPersistenceTests(PostgresDatabase database)
{
    [PostgresFact]
    [Trait("Suite", "Persistence")]
    public void ManagerOrderForm_SavesCustomerCargoAndBothRoutePoints()
    {
        using var ui = database.OpenSession("qa_fixture_manager");
        var form = ui.RenderComponent<ManagerDashboard>();
        FillOrder(form, "QA Збереження", "3", "-12", "-4");
        form.ClickButton("Створити замовлення");

        using var db = database.CreateDbContext();
        var order = db.Orders.AsNoTracking()
            .Include(o => o.OrderCargos).ThenInclude(link => link.Cargo)
            .Include(o => o.OrderPoints)
            .Single(o => o.CustomerCompany == "QA Збереження");
        var link = Assert.Single(order.OrderCargos);
        var points = order.OrderPoints.OrderBy(point => point.SequenceNumber).ToArray();
        Assert.Equal(2, points.Length);
        CheckSavedFields(
            ("контакт замовника", order.CustomerName == "Тестова Особа"),
            ("телефон замовника", order.CustomerPhone == "0500000914"),
            ("email замовника", order.CustomerEmail == "order@example.test"),
            ("назва вантажу", link.Cargo.Title == "QA Продукти"),
            ("кількість", link.Quantity == 3),
            ("вага одиниці", link.Cargo.WeightKg == 2500),
            ("об’єм одиниці", link.Cargo.VolumeM3 == 4.5),
            ("загальна вага", order.WeightTons == 7.5),
            ("загальний об’єм", order.VolumeM3 == 13.5),
            ("рефрижератор", link.Cargo.NeedsRefrigerator && order.RequiresRefrigerator),
            ("мінімальна температура", link.Cargo.MinTemperatureC == -12),
            ("максимальна температура", link.Cargo.MaxTemperatureC == -4),
            ("особливі умови", link.Cargo.SpecialConditions == "QA Не кантувати"),
            ("пункт завантаження: тип, порядок, адреса, контакт, телефон",
                points[0].PointType == "Завантаження" && points[0].SequenceNumber == 1 &&
                points[0].Address == "QA Склад" && points[0].ContactName == "QA Складський" &&
                points[0].ContactPhone == "0500000915"),
            ("пункт доставки: тип, порядок, адреса, контакт, телефон",
                points[1].PointType == "Доставка" && points[1].SequenceNumber == 2 &&
                points[1].Address == "QA Офіс" && points[1].ContactName == "QA Одержувач" &&
                points[1].ContactPhone == "0500000916"));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    public void ManagerOrderForm_InvalidEmailDoesNotCreateOrder()
    {
        using var ui = database.OpenSession("qa_fixture_manager");
        var form = ui.RenderComponent<ManagerDashboard>();
        FillOrder(form, "QA Невірний email", "3", "-12", "-4");
        SelectOrderTab(form, "1.");
        form.Fill("company@gmail.com", "invalid");
        form.ClickButton("Створити замовлення");
        Assert.NotEmpty(form.FindAll(".invalid-feedback-modern"));

        using var db = database.CreateDbContext();
        Assert.False(db.Orders.Any(o => o.CustomerCompany == "QA Невірний email"));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public void ManagerOrderForm_RejectsZeroQuantity()
    {
        AssertInvalidOrderIsNotSaved("QA Нульова кількість", "0", "-12", "-4");
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public void ManagerOrderForm_RejectsInvertedTemperatureRange()
    {
        AssertInvalidOrderIsNotSaved("QA Некоректна температура", "3", "-4", "-12");
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    public void ManagerOrderEdit_SavesChangedCompanyAndContact()
    {
        var draft = CreateOrder(database, "QA До редагування");
        using var ui = database.OpenSession("qa_fixture_manager");
        var form = ui.RenderComponent<ManagerDashboard>();
        ClickEdit(form, draft);
        form.Fill("ТОВ «Логістик+»", "QA Після редагування");
        form.Fill("Петренко О. В.", "QA Оновлений контакт");
        form.ClickButton($"Зберегти замовлення №{draft.Id}");

        using var db = database.CreateDbContext();
        var order = db.Orders.Single(o => o.Id == draft.Id);
        CheckSavedFields(("компанія", order.CustomerCompany == "QA Після редагування"),
            ("контакт", order.CustomerName == "QA Оновлений контакт"));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public void OrderServiceUpdate_ContactChangeDoesNotEraseDriverArrival()
    {
        var draft = CreateOrder(database, "QA Зберегти прогрес");
        var orders = new OrderService(database);
        var point = orders.GetPointProgress(draft.Id)[0];
        Assert.True(orders.SetPointStatus(point.Id, "Прибув"));

        draft.CustomerName = "QA Новий контакт";
        draft.Status = "В роботі";
        orders.UpdateOrder(draft);

        using var db = database.CreateDbContext();
        var saved = db.OrderPoints.Single(p => p.OrderId == draft.Id && p.SequenceNumber == 1);
        CheckSavedFields(("статус прибуття", saved.Status == "Прибув"),
            ("дата фактичного прибуття", saved.ActualArrival.HasValue));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    public void DriverPointConfirmation_SavesStatusTimestampAndHistory()
    {
        var draft = CreateOrder(database, "QA Запис прибуття");
        var orders = new OrderService(database);
        var point = orders.GetPointProgress(draft.Id)[0];

        Assert.False(orders.SetPointStatus(point.Id, "Завантажено"));
        Assert.True(orders.SetPointStatus(point.Id, "Прибув"));
        Assert.False(orders.SetPointStatus(point.Id, "Прибув"));

        using var db = database.CreateDbContext();
        var saved = db.OrderPoints.Single(p => p.Id == point.Id);
        Assert.Equal("Прибув", saved.Status);
        Assert.NotNull(saved.ActualArrival);
        Assert.Equal("В роботі", db.Orders.Single(o => o.Id == draft.Id).Status);
        Assert.Single(db.OrderStatusHistories.Where(h => h.OrderId == draft.Id && h.NewStatus == "Прибув"));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public void HistoryPage_ShowsAnOrderSavedInPostgres()
    {
        var draft = CreateOrder(database, "QA Запис з PostgreSQL для історії");
        Assert.True(new OrderService(database).SetOrderStatus(draft.Id, "В роботі", "QA Зміна для історії"));
        using var ui = database.OpenSession("qa_fixture_manager");
        var history = ui.RenderComponent<History>();
        Assert.Contains(draft.CustomerCompany, history.Markup);
    }

    internal static OrderDraft CreateOrder(PostgresDatabase database, string company)
    {
        var draft = new OrderDraft
        {
            CustomerCompany = company, CustomerName = "Тестова Особа", CustomerPhone = "0500000914",
            CustomerEmail = "order@example.test", Status = "Нове",
            Cargos = [new() { Name = "QA Вантаж", WeightTons = 2, VolumeM3 = 5, Quantity = 2 }],
            Points =
            [
                new() { IsLoading = true, Address = "QA Склад", ContactName = "Складський", ContactPhone = "0500000915" },
                new() { IsLoading = false, Address = "QA Офіс", ContactName = "Одержувач", ContactPhone = "0500000916" }
            ]
        };
        new OrderService(database).AddOrder(draft);
        return draft;
    }

    private void AssertInvalidOrderIsNotSaved(string company, string quantity, string lower, string upper)
    {
        using var ui = database.OpenSession("qa_fixture_manager");
        var form = ui.RenderComponent<ManagerDashboard>();
        FillOrder(form, company, quantity, lower, upper);
        var error = Record.Exception(() => form.ClickButton("Створити замовлення"));

        using var db = database.CreateDbContext();
        Assert.False(db.Orders.Any(o => o.CustomerCompany == company),
            $"Замовлення «{company}» збережено з некоректними значеннями.");
        Assert.Null(error);
        Assert.NotEmpty(form.FindAll(".invalid-feedback-modern"));
    }

    private static void ClickEdit(IRenderedFragment form, OrderDraft draft)
    {
        form.FindAll("button").Where(button => button.TextContent.Trim() == "Редагувати")
            .Single(button => button.Closest(".cargo-card")?.TextContent.Contains(draft.CustomerCompany) == true)
            .Click();
    }

    private static void FillOrder(IRenderedFragment form, string company, string quantity, string lower, string upper)
    {
        form.Fill("ТОВ «Логістик+»", company);
        form.Fill("Петренко О. В.", "Тестова Особа");
        form.Fill("0501234567", "0500000914");
        form.Fill("company@gmail.com", "order@example.test");
        SelectOrderTab(form, "2.");
        form.Fill("Продукти харчування", "QA Продукти");
        form.Fill("10", quantity);
        form.Fill("2.5", "2.5");
        form.Fill("12", "4.5");
        form.Find(".cargo-card input[type=checkbox]").Change(true);
        form.Fill("-18", lower);
        form.Fill("-5", upper);
        form.Fill("Крихкий, не кантувати", "QA Не кантувати");
        SelectOrderTab(form, "3.");
        form.ClickButton("Завантаження");
        form.ClickButton("Доставка");
        form.Fill("вул. Хрещатик, 1", "QA Склад");
        form.Fill("Петренко О. В.", "QA Складський");
        form.Fill("0501234567", "0500000915");
        form.Fill("вул. Хрещатик, 1", "QA Офіс", 1);
        form.Fill("Петренко О. В.", "QA Одержувач", 1);
        form.Fill("0501234567", "0500000916", 1);
    }

    private static void SelectOrderTab(IRenderedFragment form, string prefix) =>
        form.FindAll("button.nav-tab-modern").Single(b => b.TextContent.TrimStart()
            .StartsWith(prefix, StringComparison.Ordinal)).Click();
}
