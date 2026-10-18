using Estacionamiento_grupo_9.Data;
using Estacionamiento_grupo_9.Models;
using Estacionamiento_grupo_9.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IRepositorioCliente, RepositorioCliente>();
builder.Services.AddScoped<IRepositorioPlaza, RepositorioPlaza>();
builder.Services.AddScoped<IRepositorioTipoDePlaza, RepositorioTipoDePlaza>();
builder.Services.AddScoped<IRepositorioEstadia, RepositorioEstadia>();

// --- Identity (Staff) ---
var connectionString = builder.Configuration.GetConnectionString("MySql")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'MySql' en appsettings.json.");

// Versión de MySQL fija para no necesitar conexión al arrancar.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services.AddIdentity<Staff, IdentityRole<int>>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<StaffClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Staff/Login";
    options.AccessDeniedPath = "/Staff/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<IAvatarService, AvatarService>();

var app = builder.Build();

// Migraciones pendientes + roles (Admin, Operador) + admin inicial
using (var scope = app.Services.CreateScope())
{
    try
    {
        await SeedData.InicializarAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Falló la inicialización de Identity. ¿Está la base levantada y creaste la migración (dotnet ef migrations add)?");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
// Necesario para servir los avatares subidos en tiempo de ejecución (wwwroot/uploads).
// MapStaticAssets solo conoce los archivos que existían al compilar.
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
