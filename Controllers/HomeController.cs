using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.ViewModels;

namespace Rental.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = Rental.Services.Roles.DashboardArea)]
public class HomeController : Controller
{
    private readonly RentalContext _context;
    private readonly IWebHostEnvironment _env;

    public HomeController(RentalContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        // Anonymous visitors get the public marketing homepage; signed-in staff
        // go straight to the dashboard.
        if (User.Identity?.IsAuthenticated != true)
        {
            return View("Landing");
        }

        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        var viewModel = new DashboardViewModel
        {
            PropertyCount = await _context.Properties.CountAsync(),
            ActiveLeaseCount = await _context.Leases.CountAsync(l => l.EndDate == null || l.EndDate >= today),
            TotalMonthlyRent = await _context.Leases
                .Select(l => (decimal?)l.MonthlyRent)
                .SumAsync() ?? 0m,
            PaymentsCollectedThisMonth = await _context.Payments
                .Where(p => p.PaymentDate >= monthStart && p.PaymentDate < nextMonth)
                .Select(p => (decimal?)p.Amount)
                .SumAsync() ?? 0m,
            UpcomingRenewals = await _context.Leases
                .Include(l => l.Property)
                .Include(l => l.Tenant)
                .Where(l => l.EndDate != null && l.EndDate >= today && l.EndDate <= today.AddMonths(2))
                .OrderBy(l => l.EndDate)
                .Select(l => new DashboardViewModel.LeaseSummary
                {
                    PropertyName = l.Property!.Name,
                    TenantName = l.Tenant!.FullName,
                    EndDate = l.EndDate,
                    MonthlyRent = l.MonthlyRent,
                    Status = l.Status
                })
                .ToListAsync()
        };

        return View(viewModel);
    }

    /// <summary>
    /// Target of the production exception handler (see Program.cs). Returns a
    /// plain JSON 500 so API clients get a clean response instead of a 404.
    /// </summary>
    [AllowAnonymous]
    public IActionResult Error()
    {
        Response.StatusCode = 500;
        return Json(new { message = "An unexpected error occurred." });
    }

    /// <summary>
    /// Public download of the Android APK linked from the landing page.
    /// Served with the APK content type so browsers save it as an installable file.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/download/app")]
    public IActionResult DownloadApp()
    {
        var path = Path.Combine(_env.WebRootPath, "downloads", "rental-management.apk");
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        return PhysicalFile(path, "application/vnd.android.package-archive", "RentalManagement-v1.0.1.apk");
    }
}
