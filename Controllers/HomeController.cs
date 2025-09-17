using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.ViewModels;

namespace Rental.Controllers;

public class HomeController : Controller
{
    private readonly RentalContext _context;

    public HomeController(RentalContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        var viewModel = new DashboardViewModel
        {
            PropertyCount = await _context.Properties.CountAsync(),
            TenantCount = await _context.Tenants.CountAsync(),
            ActiveLeaseCount = await _context.Leases.CountAsync(l => l.EndDate == null || l.EndDate >= today),
            TotalMonthlyRent = await _context.Leases
                .Select(l => (decimal?)l.MonthlyRent)
                .SumAsync() ?? 0m,
            PaymentsCollectedThisMonth = await _context.Payments
                .Where(p => p.PaymentDate >= monthStart && p.PaymentDate < nextMonth)
                .Select(p => (decimal?)p.Amount)
                .SumAsync() ?? 0m
        };

        return View(viewModel);
    }
}
