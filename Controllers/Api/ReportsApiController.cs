using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental.Api;
using Rental.Data;

namespace Rental.Controllers.Api;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsApiController : ControllerBase
{
    private readonly RentalContext _context;

    public ReportsApiController(RentalContext context)
    {
        _context = context;
    }

    private async Task<(decimal totalRevenue, decimal totalExpenses, double occupancyRate)> ComputeMetricsAsync()
    {
        var transactions = await _context.Transactions.ToListAsync();
        var totalRevenue = transactions.Where(t => string.Equals(t.Type, "Income", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);
        var totalExpenses = transactions.Where(t => !string.Equals(t.Type, "Income", StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);

        var totalUnits = await _context.Properties.SumAsync(p => p.Units);
        var activeLeases = await _context.Leases.CountAsync(l => l.Status == "Active");
        var occupancyRate = totalUnits > 0
            ? Math.Round((double)activeLeases / totalUnits * 100, 1)
            : 0.0;

        return (totalRevenue, totalExpenses, occupancyRate);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var (totalRevenue, totalExpenses, occupancyRate) = await ComputeMetricsAsync();
        return Ok(new
        {
            totalRevenue,
            totalExpenses,
            netIncome = totalRevenue - totalExpenses,
            occupancyRate
        });
    }

    [HttpGet("income")]
    public async Task<IActionResult> Income()
    {
        var (totalRevenue, totalExpenses, occupancyRate) = await ComputeMetricsAsync();
        return Ok(new
        {
            totalRevenue,
            totalExpenses,
            netIncome = totalRevenue - totalExpenses,
            occupancyRate
        });
    }

    [HttpGet("expenses")]
    public async Task<IActionResult> Expenses()
    {
        var (totalRevenue, totalExpenses, occupancyRate) = await ComputeMetricsAsync();
        return Ok(new
        {
            totalRevenue,
            totalExpenses,
            netIncome = totalRevenue - totalExpenses,
            occupancyRate
        });
    }

    [HttpGet("occupancy")]
    public async Task<IActionResult> Occupancy()
    {
        var (totalRevenue, totalExpenses, occupancyRate) = await ComputeMetricsAsync();
        return Ok(new
        {
            totalRevenue,
            totalExpenses,
            netIncome = totalRevenue - totalExpenses,
            occupancyRate
        });
    }

    [HttpGet("lease-expirations")]
    public async Task<IActionResult> LeaseExpirations([FromQuery] int days = 90)
    {
        var now = DateTime.Today;
        var cutoff = now.AddDays(days);

        var expiring = await _context.Leases
            .Where(l => l.EndDate != null && l.EndDate >= now && l.EndDate <= cutoff)
            .OrderBy(l => l.EndDate)
            .Select(l => new
            {
                id = l.Id,
                tenantId = l.TenantId,
                tenantName = l.Tenant != null ? l.Tenant.FirstName + " " + l.Tenant.LastName : null,
                propertyId = l.PropertyId,
                propertyName = l.Property != null ? l.Property.Name : null,
                endDate = l.EndDate,
                monthlyRent = l.MonthlyRent,
                status = l.Status
            })
            .ToListAsync();

        return Ok(new { windowDays = days, count = expiring.Count, leases = expiring });
    }

    [HttpGet("maintenance")]
    public async Task<IActionResult> Maintenance()
    {
        var requests = await _context.MaintenanceRequests.ToListAsync();
        return Ok(new
        {
            total = requests.Count,
            open = requests.Count(m => m.Status == "Open"),
            inProgress = requests.Count(m => m.Status == "In Progress"),
            completed = requests.Count(m => m.Status == "Completed"),
            estimatedCost = requests.Sum(m => m.EstimatedCost ?? 0m)
        });
    }

    [HttpPost("export")]
    public IActionResult Export([FromBody] ExportReportRequest request)
    {
        return Ok(new
        {
            status = "ok",
            reportType = request.ReportType ?? "summary",
            generatedAt = DateTime.UtcNow,
            note = "Export placeholder — no file generation configured."
        });
    }
}
