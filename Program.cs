using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rental.Data;
using Rental.Services;

// Kenyan Shilling everywhere: currency renders as "KSh 75,500.00", matching the mobile app.
var kenyanCulture = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.GetCultureInfo("en-KE").Clone();
kenyanCulture.NumberFormat.CurrencySymbol = "KSh";
kenyanCulture.NumberFormat.CurrencyPositivePattern = 2; // "KSh 75,500.00"
kenyanCulture.NumberFormat.CurrencyNegativePattern = 9; // "-KSh 75,500.00"
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = kenyanCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = kenyanCulture;
System.Globalization.CultureInfo.CurrentCulture = kenyanCulture;
System.Globalization.CultureInfo.CurrentUICulture = kenyanCulture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var connectionString = builder.Configuration.GetConnectionString("RentalDatabase")
    ?? "Server=(localdb)\\mssqllocaldb;Database=RentalDb;Trusted_Connection=True;TrustServerCertificate=True;";
builder.Services.AddDbContext<RentalContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// JWT authentication for the /api/* endpoints used by the Flutter app.
builder.Services.AddSingleton<JwtService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-only-super-secret-key-please-change-1234567890";
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "RentalAdmin",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "RentalApp",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = System.TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Apply the Kenyan culture to every request (deterministic per-request culture).
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(kenyanCulture),
    SupportedCultures = new[] { kenyanCulture },
    SupportedUICultures = new[] { kenyanCulture }
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<RentalContext>();

    // Dev helper: if the database predates the API tables, recreate the demo schema.
    try
    {
        _ = await context.Users.CountAsync();
    }
    catch
    {
        await context.Database.EnsureDeletedAsync();
    }

    await context.Database.EnsureCreatedAsync();
    await SeedData.EnsureSeededAsync(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// HTTPS redirection is skipped in Development so the Flutter app can reach
// the plain HTTP endpoint (e.g. from an Android emulator via 10.0.2.2).
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

await app.RunAsync();
