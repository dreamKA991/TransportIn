using TransportIn.Components;
using Microsoft.EntityFrameworkCore;
using TransportIn.Data;
using TransportIn.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<TransportIn.Services.AuthService>();
builder.Services.AddSingleton<TransportIn.Services.EmergencyService>();
builder.Services.AddScoped<TransportIn.Services.FleetService>();
builder.Services.AddScoped<TransportIn.Services.OrderService>();
builder.Services.AddScoped<TransportIn.Services.ExpertRulesService>();
builder.Services.AddScoped<EmployeeService>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var connection = db.Database.GetDbConnection();

    Console.WriteLine("œŒƒ Àﬁ◊≈ÕÕ¿ﬂ ¡¿«¿: " + connection.Database);
    Console.WriteLine("œŒÀ‹«Œ¬¿“≈À‹: " + connection.DataSource);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
