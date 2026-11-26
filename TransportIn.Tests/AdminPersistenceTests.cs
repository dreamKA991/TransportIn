using Bunit;
using Microsoft.EntityFrameworkCore;
using TransportIn.Components.Pages;
using TransportIn.Services;
using static TransportIn.Tests.Persistence.FormActions;

namespace TransportIn.Tests.Persistence;

[Collection("PostgreSQL")]
public class AdminPersistenceTests(PostgresDatabase database)
{
    [PostgresFact]
    [Trait("Suite", "Persistence")]
    public void VehicleForm_SavesCoreFields()
    {
        using var ui = database.OpenSession("qa_fixture_admin");
        var form = ui.RenderComponent<AdminDashboard>();
        FillVehicle(form, "AA 7310 BP", "WVWZZZ1JZXW000101");
        form.ClickButton("Додати авто");

        using var db = database.CreateDbContext();
        var vehicle = db.Vehicles.AsNoTracking().Single(v => v.LicensePlate == "AA 7310 BP");
        CheckSavedFields(
            ("марка і модель", vehicle.BrandModel == "QA Volvo"),
            ("держномер", vehicle.LicensePlate == "AA 7310 BP"),
            ("VIN", vehicle.VinCode == "WVWZZZ1JZXW000101"),
            ("тип кузова", vehicle.BodyType == "Ізотерм"),
            ("водій", vehicle.AssignedDriver == PostgresDatabase.DriverName),
            ("вантажопідйомність", vehicle.CapacityTons == 15.5),
            ("об’єм", vehicle.VolumeM3 == 81),
            ("холодильне обладнання", vehicle.HasRefrigerator));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public void VehicleForm_SavesDimensionsEquipmentScheduleAndDocuments()
    {
        using var ui = database.OpenSession("qa_fixture_admin");
        var form = ui.RenderComponent<AdminDashboard>();
        FillVehicle(form, "AA 7311 BP", "WVWZZZ1JZXW000102");
        form.ClickButton("Документи");
        form.Fill("CXX 123456", "AAA 654321");
        form.Fill("12345-02123-26", "QA-INSPECT-1");
        form.Fill("ДД.ММ.РРРР", "01.12.2030");
        form.Fill("№ 1234/Л", "QA-LICENSE-1");
        form.SelectWithOption("Недійсна").Change("Недійсна");
        form.Fill("Серія/номер поліса", "QA-INSURANCE-1");
        form.ClickButton("Додати авто");

        using var db = database.CreateDbContext();
        var vehicle = db.Vehicles.AsNoTracking()
            .Include(v => v.VehicleDocuments).Include(v => v.VehicleEquipments)
            .Single(v => v.LicensePlate == "AA 7311 BP");
        var driverId = db.Employees.Single(e => e.FullName == PostgresDatabase.DriverName).Id;
        var docs = vehicle.VehicleDocuments;
        CheckSavedFields(
            ("графік водія", db.DriverSchedules.Any(s => s.EmployeeId == driverId && s.ShiftType == "Вечірній")),
            ("зовнішня довжина", vehicle.ExteriorLengthMeters == 12.5),
            ("зовнішня ширина", vehicle.ExteriorWidthMeters == 2.5),
            ("зовнішня висота", vehicle.ExteriorHeightMeters == 3.9),
            ("внутрішня довжина", vehicle.InteriorLengthMeters == 12.1),
            ("внутрішня ширина", vehicle.InteriorWidthMeters == 2.4),
            ("внутрішня висота", vehicle.InteriorHeightMeters == 2.6),
            ("власна маса", vehicle.EmptyWeightKg == 7100),
            ("допустима маса", vehicle.MaxGrossWeightKg == 38000),
            ("ремені кріплення", vehicle.VehicleEquipments.Any(e => e.Name.Contains("ремен", StringComparison.OrdinalIgnoreCase))),
            ("техпаспорт", docs.Any(d => d.DocumentNumber == "AAA 654321")),
            ("номер техогляду", docs.Any(d => d.DocumentNumber == "QA-INSPECT-1")),
            ("дата техогляду", docs.Any(d => d.DocumentNumber == "QA-INSPECT-1" &&
                (d.IssueDate == new DateOnly(2030, 12, 1) || d.ExpiryDate == new DateOnly(2030, 12, 1)))),
            ("номер ліцензії", docs.Any(d => d.DocumentNumber == "QA-LICENSE-1")),
            ("статус ліцензії", docs.Any(d => d.DocumentNumber == "QA-LICENSE-1" && d.Status == "Недійсна")),
            ("страховка", docs.Any(d => d.DocumentNumber == "QA-INSURANCE-1")));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public void VehicleEdit_SavesTheChangePromisedByTheUi()
    {
        const string plate = "AA 7312 BP";
        new FleetService(database).AddVehicle(new TransportIn.Models.Vehicle
        {
            BrandModel = "QA before", LicensePlate = plate, VinCode = "WVWZZZ1JZXW000103",
            BodyType = "Тент", CapacityTons = 10, VolumeM3 = 50
        });
        using var ui = database.OpenSession("qa_fixture_admin");
        var form = ui.RenderComponent<AdminDashboard>();
        form.FindAll("tr").Single(row => row.TextContent.Contains(plate)).QuerySelector("button[title='Редагувати']")!.Click();
        form.Fill("Volvo FH16", "QA after");
        form.ClickButton("Зберегти зміни");
        Assert.Contains("оновлено", form.Markup);

        using var db = database.CreateDbContext();
        CheckSavedFields(("марка і модель", db.Vehicles.Single(v => v.LicensePlate == plate).BrandModel == "QA after"));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    public void EmployeeForm_SavesProfileAndHashedCredentials()
    {
        using var ui = database.OpenSession("qa_fixture_admin");
        var form = ui.RenderComponent<AdminDashboard>();
        FillEmployee(form, "qa_staff_core", "0500000911");
        form.ClickButton("Додати працівника");

        using var db = database.CreateDbContext();
        var user = db.Users.AsNoTracking().Include(u => u.EmployeeNavigation)
            .Include(u => u.RoleNavigation).Single(u => u.Username == "qa_staff_core");
        var employee = Assert.IsType<TransportIn.Data.Entities.Employee>(user.EmployeeNavigation);
        CheckSavedFields(
            ("ПІБ", employee.FullName == "Тестенко Олена QA"),
            ("роль", employee.Role == "Водій" && user.RoleNavigation.Name == "Водій"),
            ("телефон", employee.Phone == "0500000911"),
            ("email", employee.Email == "qa_staff_core@example.test"),
            ("категорії посвідчення", employee.LicenseCategory == "CE"),
            ("строк медкнижки", employee.MedicalCertExpiry == "01.12.2030"),
            ("хеш пароля", user.PasswordHash != "Synthetic-QA-2026!" &&
                PasswordHasher.Verify("Synthetic-QA-2026!", user.PasswordHash)));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public void EmployeeForm_SavesTaxIdAndEnteredDocuments()
    {
        using var ui = database.OpenSession("qa_fixture_admin");
        var form = ui.RenderComponent<AdminDashboard>();
        FillEmployee(form, "qa_staff_docs", "0500000912");
        form.ClickButton("Додати працівника");

        using var db = database.CreateDbContext();
        var employee = db.Users.Include(u => u.EmployeeNavigation!).ThenInclude(e => e.EmployeeDocuments)
            .Single(u => u.Username == "qa_staff_docs").EmployeeNavigation!;
        var docs = employee.EmployeeDocuments;
        CheckSavedFields(
            ("РНОКПП", employee.TaxId == "3012345678"),
            ("номер ID-картки", docs.Any(d => d.DocumentNumber == "987654321")),
            ("УНЗР", docs.Any(d => d.UniqueRecordNumber == "19900101-12345")),
            ("тип і номер трудової", docs.Any(d => d.DocumentNumber.Contains("7654321"))),
            ("серія диплома", docs.Any(d => d.DocumentNumber.Contains("654321") || d.DocumentSeries == "ВК")),
            ("заклад освіти", docs.Any(d => d.InstitutionName == "QA Університет")),
            ("рік закінчення", docs.Any(d => d.GraduationYear == 2021)),
            ("номер медкнижки", docs.Any(d => d.DocumentNumber == "QA-MED-01")),
            ("номер посвідчення водія", docs.Any(d => d.DocumentNumber.Contains("654322"))),
            ("строк посвідчення", docs.Any(d => d.DocumentNumber.Contains("654322") &&
                d.ExpiryDate.HasValue && d.ExpiryDate.Value.Date == new DateTime(2031, 12, 1))),
            ("військовий документ", docs.Any(d => d.DocumentNumber.Contains("654323"))),
            ("ВУС", docs.Any(d => d.MilitarySpecialtyCode == "837")),
            ("звання", docs.Any(d => d.MilitaryRank == "Солдат")));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public void EmployeeEdit_SavesTheChangePromisedByTheUi()
    {
        var employee = new Employee
        {
            FullName = "Тестенко Працівник", Role = "Водій", Phone = "0500000913",
            LicenseCategory = "C", MedicalCertExpiry = "01.12.2030"
        };
        new EmployeeService(database).AddEmployee(employee);
        using var ui = database.OpenSession("qa_fixture_admin");
        var form = ui.RenderComponent<AdminDashboard>();
        form.FindAll("tr").Single(row => row.TextContent.Contains(employee.Phone))
            .QuerySelector("button[title='Редагувати']")!.Click();
        form.Fill("Іванов", "Оновлено");
        form.ClickButton("Зберегти зміни");
        Assert.Contains("оновлено", form.Markup);

        using var db = database.CreateDbContext();
        CheckSavedFields(("нове прізвище", db.Employees.Single(e => e.Id == employee.Id)
            .FullName.StartsWith("Оновлено", StringComparison.Ordinal)));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    public void ExpertRuleForm_SavesAllFourFields()
    {
        using var ui = database.OpenSession("qa_fixture_admin");
        var form = ui.RenderComponent<AdminDashboard>();
        form.Fill("Повітряна тривога", "QA Снігопад");
        form.Fill("Тривога в регіоні", "QA ТЗ у дорозі");
        form.Fill("Зупинити рух", "QA Повідомити менеджера");
        form.SelectWithOption("Середній").Change("Середній");
        form.ClickButton("Додати");

        using var db = database.CreateDbContext();
        var rule = db.ExpertRules.Single(r => r.ThreatType == "QA Снігопад");
        CheckSavedFields(
            ("умова", rule.Condition == "QA ТЗ у дорозі"),
            ("рекомендація", rule.ActionRecommendation == "QA Повідомити менеджера"),
            ("пріоритет", rule.Priority == "Середній"));
    }

    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public void RoadEventForm_SavesAddressAndEventType()
    {
        using var ui = database.OpenSession("qa_fixture_admin");
        var form = ui.RenderComponent<AdminDashboard>();
        form.SelectWithOption("Ремонт дороги").Change("Ремонт дороги");
        form.Field("Місце (напр. вул. Хрещатик, 1)").Input("QA Вулиця 103");
        form.ClickButton("Додати подію");
        Assert.Contains("QA Вулиця 103", form.Markup);

        using var db = database.CreateDbContext();
        CheckSavedFields(("адреса і тип дорожньої події", db.RoadEvents
            .Any(e => e.Address == "QA Вулиця 103" && e.EventType == "Ремонт дороги")));
    }

    private static void FillVehicle(IRenderedFragment form, string plate, string vin)
    {
        form.Fill("Volvo FH16", "QA Volvo");
        form.Fill("AA 1234 BP", plate);
        form.Fill("1YVBR12E34567890X", vin);
        form.SelectWithOption("Ізотерм").Change("Ізотерм");
        form.SelectWithOption(PostgresDatabase.DriverName).Change(PostgresDatabase.DriverName);
        form.SelectWithOption("Вечірній").Change("Вечірній");
        form.ClickButton("Габарити та маси");
        form.Fill("13.6", "12.5");
        form.Fill("2.55", "2.5");
        form.Fill("4.0", "3.9");
        form.Fill("13.62", "12.1");
        form.Fill("2.48", "2.4");
        form.Fill("2.70", "2.6");
        form.Fill("22", "15.5");
        form.Fill("86", "81");
        form.Fill("7500", "7100");
        form.Fill("40000", "38000");
        form.FindAll("input[type=checkbox]")[0].Change(true);
        form.FindAll("input[type=checkbox]")[1].Change(true);
    }

    private static void FillEmployee(IRenderedFragment form, string login, string phone)
    {
        form.Fill("Іванов", "Тестенко");
        form.Fill("Іван", "Олена");
        form.Fill("Іванович", "QA");
        form.SelectWithOption("Водій").Change("Водій");
        form.Fill("ivanov_i", login);
        form.Fill("••••••", "Synthetic-QA-2026!");
        form.Fill("0501234567", phone);
        form.Fill("name@gmail.com", $"{login}@example.test");
        form.ClickButton("Паспорт і податки");
        form.SelectWithOption("ID-картка").Change("ID-картка");
        form.Fill("АН 123456 або 123456789", "987654321");
        form.Fill("19900101-12345", "19900101-12345");
        form.Fill("3012345678", "3012345678");
        form.ClickButton("Трудова / навчання");
        form.SelectWithOption("Електронна").Change("Електронна");
        form.Fill("БТ 7654321", "БТ 7654321");
        form.Fill("ВК 123456", "ВК 654321");
        form.Fill("КНУТЕУ", "QA Університет");
        form.Fill("2020", "2021");
        form.ClickButton("Медичні / водійські");
        form.Fill("1234", "QA-MED-01");
        form.Fill("ДД.ММ.РРРР", "01.12.2030");
        form.Fill("ABC 123456", "AAA 654322");
        form.Fill("C, CE", "CE");
        form.Fill("ДД.ММ.РРРР", "01.12.2031", 1);
        form.ClickButton("Військові");
        form.Fill("ВО 123456", "ВО 654323");
        form.Fill("837", "837");
        form.Fill("Солдат", "Солдат");
    }
}

[CollectionDefinition("PostgreSQL")]
public class PostgresCollection : ICollectionFixture<PostgresDatabase>;
