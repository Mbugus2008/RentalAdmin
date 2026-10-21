using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

builder.Services.AddMemoryCache();

// The admin console signs staff in with cookie sessions, while the Flutter app
// keeps using JWT bearer tokens on /api/*. One policy scheme forwards each
// request to the right handler based on its path.
builder.Services.AddSingleton<JwtService>();

const string adminOrApiScheme = "AdminOrApi";

var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-only-super-secret-key-please-change-1234567890";
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = adminOrApiScheme;
        options.DefaultChallengeScheme = adminOrApiScheme;
    })
    .AddPolicyScheme(adminOrApiScheme, "Cookie for the admin console, JWT for the API", options =>
    {
        options.ForwardDefaultSelector = context =>
            context.Request.Path.StartsWithSegments("/api")
                ? JwtBearerDefaults.AuthenticationScheme
                : CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "RentalAdmin.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        options.ExpireTimeSpan = System.TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    })
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

builder.Services.AddAuthorization(options =>
{
    // The admin console is private: every page requires a signed-in user unless
    // the controller or action opts out with [AllowAnonymous].
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

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
