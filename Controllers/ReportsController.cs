using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.ViewModels;

namespace Rental.Controllers;

public class ReportsController : Controller
{
    private readonly RentalContext _context;

    public ReportsController(RentalContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new ReportsViewModel();

        // ----- Financials (mirrors /api/reports/summary) -----
        var transactions = await _context.Transactions.ToListAsync();
        model.TotalRevenue = transactions
            .Where(t => string.Equals(t.Type, "Income", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);
        model.TotalExpenses = transactions
            .Where(t => !string.Equals(t.Type, "Income", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);

        model.IncomeByCategory = transactions
            .Where(t => string.Equals(t.Type, "Income", StringComparison.OrdinalIgnoreCase))
            .GroupBy(t => t.Category)
            .Select(g => new ReportsViewModel.CategoryTotal { Category = g.Key, Amount = g.Sum(t => t.Amount) })
            .OrderByDescending(c => c.Amount)
            .ToList();

        model.ExpenseByCategory = transactions
            .Where(t => !string.Equals(t.Type, "Income", StringComparison.OrdinalIgnoreCase))
            .GroupBy(t => t.Category)
            .Select(g => new ReportsViewModel.CategoryTotal { Category = g.Key, Amount = g.Sum(t => t.Amount) })
            .OrderByDescending(c => c.Amount)
            .ToList();

        // ----- Portfolio / occupancy -----
        model.PropertiesCount = await _context.Properties.CountAsync();
        model.UnitsCount = await _context.Properties.SumAsync(p => p.Units);
        model.ActiveLeaseCount = await _context.Leases.CountAsync(l => l.Status == "Active");
        model.OccupancyRate = model.UnitsCount > 0
            ? Math.Round((double)model.ActiveLeaseCount / model.UnitsCount * 100, 1)
            : 0.0;

        // ----- Invoicing -----
        var invoices = await _context.Invoices.ToListAsync();
        model.InvoicedTotal = invoices.Sum(i => i.TotalAmount);
        model.CollectedTotal = invoices.Sum(i => i.AmountPaid);

        // ----- Outstanding balances per tenant (the app's "Balances" view) -----
        var tenantNames = await _context.Tenants
            .ToDictionaryAsync(t => t.Id, t => t.FullName ?? $"{t.FirstName} {t.LastName}".Trim());
        model.TenantBalances = invoices
            .GroupBy(i => i.TenantId)
            .Select(g => new ReportsViewModel.TenantBalance
            {
                TenantName = tenantNames.TryGetValue(g.Key, out var name) ? name : $"Tenant #{g.Key}",
                Invoiced = g.Sum(i => i.TotalAmount),
                Paid = g.Sum(i => i.AmountPaid),
                Balance = g.Sum(i => i.TotalAmount - i.AmountPaid)
            })
            .OrderByDescending(b => b.Balance)
            .ToList();

        // ----- Lease expirations (next 90 days) -----
        var today = DateTime.Today;
        var cutoff = today.AddDays(90);
        model.ExpiringLeases = await _context.Leases
            .Include(l => l.Property)
            .Include(l => l.Tenant)
            .Where(l => l.EndDate != null && l.EndDate >= today && l.EndDate <= cutoff)
            .OrderBy(l => l.EndDate)
            .Select(l => new ReportsViewModel.ExpiringLease
            {
                PropertyName = l.Property != null ? l.Property.Name : "—",
                TenantName = l.Tenant != null ? (l.Tenant.FirstName + " " + l.Tenant.LastName) : "—",
                EndDate = l.EndDate,
                MonthlyRent = l.MonthlyRent,
                Status = l.Status
            })
            .ToListAsync();

        return View(model);
    }
}
