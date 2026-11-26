using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TransportIn.Data;
using TransportIn.Services;

namespace TransportIn.Tests.Persistence;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(PostgresDatabase.ConnectionVariable)))
            Skip = $"Set {PostgresDatabase.ConnectionVariable} to a local test server.";
    }
}

public sealed class PostgresDatabase : IAsyncLifetime, IDbContextFactory<AppDbContext>
{
    public const string ConnectionVariable = "TRANSPORTIN_TEST_POSTGRES";
    internal const string DriverLogin = "qa_fixture_driver";
    internal const string DriverName = "Тестовий Водій";
    private const string TestPassword = "TransportIn-Test-Only-2026!";
    private readonly string databaseName = $"transportin_qa_{Guid.NewGuid():N}";
    private string? adminConnectionString;
    private DbContextOptions<AppDbContext>? options;
    private bool created;

    public async Task InitializeAsync()
    {
        var value = Environment.GetEnvironmentVariable(ConnectionVariable);
        if (string.IsNullOrWhiteSpace(value))
            return;

        var connection = new NpgsqlConnectionStringBuilder(value);
        if (connection.Host is not ("localhost" or "127.0.0.1" or "::1") ||
            connection.Database != "postgres")
            throw new InvalidOperationException($"{ConnectionVariable} must point to the postgres database on a local test server.");

        connection.Pooling = false;
        adminConnectionString = connection.ConnectionString;
        await ExecuteAdminCommandAsync($"CREATE DATABASE \"{databaseName}\"");
        created = true;
        try
        {
            connection.Database = databaseName;
            options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connection.ConnectionString)
                .Options;

            await using var db = CreateDbContext();
            await db.Database.MigrateAsync();
            DbSeeder.EnsureSeeded(this);
            db.Users.AddRange(
                CreateTestUser(db, "qa_fixture_admin", "Адміністратор"),
                CreateTestUser(db, "qa_fixture_manager", "Менеджер"));
            await db.SaveChangesAsync();

            var driver = new Employee
            {
                FullName = DriverName, Role = "Водій", Login = DriverLogin, Password = TestPassword,
                Phone = "0500000901", Email = "driver@example.test",
                LicenseCategory = "C", MedicalCertExpiry = "01.12.2030"
            };
            new EmployeeService(this).AddEmployee(driver);
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public AppDbContext CreateDbContext() =>
        new(options ?? throw new InvalidOperationException("Test database has not been initialized."));

    internal TestContext OpenSession(string username)
    {
        var ui = new TestContext();
        ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddSingleton<IDbContextFactory<AppDbContext>>(this);
        ui.Services.AddScoped<AuthService>();
        ui.Services.AddScoped<FleetService>();
        ui.Services.AddScoped<ExpertRulesService>();
        ui.Services.AddScoped<RoadEventService>();
        ui.Services.AddSingleton<OrderService>();
        ui.Services.AddSingleton<EmployeeService>();
        ui.Services.AddSingleton<EmergencyService>();

        var auth = ui.Services.GetRequiredService<AuthService>();
        if (!auth.TryLogin(username, TestPassword, out var error))
        {
            ui.Dispose();
            throw new InvalidOperationException($"Failed to sign in to the test app: {error}");
        }
        return ui;
    }

    public async Task DisposeAsync()
    {
        if (!created)
            return;

        await ExecuteAdminCommandAsync($"DROP DATABASE \"{databaseName}\"");
        created = false;
    }

    private static TransportIn.Data.Entities.User CreateTestUser(
        AppDbContext db, string username, string roleName) =>
        new()
        {
            Username = username,
            PasswordHash = PasswordHasher.Hash(TestPassword),
            Role = db.Roles.Single(role => role.Name == roleName).Id,
            FullName = username,
            Phone = ""
        };

    private async Task ExecuteAdminCommandAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
