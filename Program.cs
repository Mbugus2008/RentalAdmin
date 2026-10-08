using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rental.Data;
using Rental.Services;

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

app.UseHttpsRedirection();
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
