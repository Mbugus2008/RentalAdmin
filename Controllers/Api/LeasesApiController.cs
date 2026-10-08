using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental.Api;
using Rental.Data;
using Rental.Models;

namespace Rental.Controllers.Api;

[ApiController]
[Route("api/leases")]
[Authorize]
public class LeasesApiController : ControllerBase
{
    private readonly RentalContext _context;

    public LeasesApiController(RentalContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _context.Leases
            .OrderByDescending(l => l.StartDate)
            .ToListAsync();
        return Ok(items.Select(ApiMap.Lease));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var lease = await _context.Leases.FindAsync(id);
        return lease == null ? NotFound() : Ok(ApiMap.Lease(lease));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LeaseRequest request)
    {
        if (request.TenantId == null || request.TenantId <= 0)
        {
            return BadRequest(new { message = "tenantId is required." });
        }

        if (request.StartDate == null)
        {
            return BadRequest(new { message = "startDate is required." });
        }

        if (!await _context.Tenants.AnyAsync(t => t.Id == request.TenantId.Value))
        {
            return BadRequest(new { message = $"Tenant {request.TenantId} does not exist." });
        }

        var lease = new Lease
        {
            UnitId = request.UnitId,
            TenantId = request.TenantId.Value,
            PropertyId = request.PropertyId,
            StartDate = request.StartDate.Value,
            EndDate = request.EndDate,
            MonthlyRent = request.MonthlyRent ?? 0m,
            SecurityDeposit = request.SecurityDeposit ?? 0m,
            LeaseTerms = request.LeaseTerms,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Leases.Add(lease);
        await _context.SaveChangesAsync();

        return StatusCode(201, ApiMap.Lease(lease));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] LeaseRequest request)
    {
        var lease = await _context.Leases.FindAsync(id);
        if (lease == null)
        {
            return NotFound();
        }

        lease.UnitId = request.UnitId ?? lease.UnitId;
        lease.PropertyId = request.PropertyId ?? lease.PropertyId;
        lease.StartDate = request.StartDate ?? lease.StartDate;
        lease.EndDate = request.EndDate ?? lease.EndDate;
        lease.MonthlyRent = request.MonthlyRent ?? lease.MonthlyRent;
        lease.SecurityDeposit = request.SecurityDeposit ?? lease.SecurityDeposit;
        lease.LeaseTerms = request.LeaseTerms ?? lease.LeaseTerms;
        lease.Status = string.IsNullOrWhiteSpace(request.Status) ? lease.Status : request.Status.Trim();
        lease.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiMap.Lease(lease));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var lease = await _context.Leases.FindAsync(id);
        if (lease == null)
        {
            return NotFound();
        }

        _context.Leases.Remove(lease);
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }
}
