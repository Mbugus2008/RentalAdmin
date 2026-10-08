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
[Route("api/tenants")]
[Authorize]
public class TenantsApiController : ControllerBase
{
    private readonly RentalContext _context;

    public TenantsApiController(RentalContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _context.Tenants.OrderBy(t => t.LastName).ThenBy(t => t.FirstName).ToListAsync();
        return Ok(items.Select(ApiMap.Tenant));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        return tenant == null ? NotFound() : Ok(ApiMap.Tenant(tenant));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TenantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(new { message = "First name and last name are required." });
        }

        var tenant = new Tenant
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.PhoneNumber?.Trim(),
            EmergencyContactName = request.EmergencyContactName?.Trim(),
            EmergencyContactPhone = request.EmergencyContactPhone?.Trim(),
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        return StatusCode(201, ApiMap.Tenant(tenant));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TenantRequest request)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            tenant.FirstName = request.FirstName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            tenant.LastName = request.LastName.Trim();
        }

        tenant.Email = request.Email?.Trim() ?? tenant.Email;
        tenant.Phone = request.PhoneNumber?.Trim() ?? tenant.Phone;
        tenant.EmergencyContactName = request.EmergencyContactName?.Trim() ?? tenant.EmergencyContactName;
        tenant.EmergencyContactPhone = request.EmergencyContactPhone?.Trim() ?? tenant.EmergencyContactPhone;
        tenant.Notes = request.Notes ?? tenant.Notes;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiMap.Tenant(tenant));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
        {
            return NotFound();
        }

        _context.Tenants.Remove(tenant);
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }
}
