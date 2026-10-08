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
[Route("api/maintenance")]
[Authorize]
public class MaintenanceApiController : ControllerBase
{
    private readonly RentalContext _context;

    public MaintenanceApiController(RentalContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _context.MaintenanceRequests
            .OrderByDescending(m => m.RequestDate)
            .ToListAsync();
        return Ok(items.Select(ApiMap.Maintenance));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var request = await _context.MaintenanceRequests.FindAsync(id);
        return request == null ? NotFound() : Ok(ApiMap.Maintenance(request));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MaintenanceRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "Title is required." });
        }

        if (request.PropertyId == null || request.PropertyId <= 0)
        {
            return BadRequest(new { message = "propertyId is required." });
        }

        var maintenance = new MaintenanceRequest
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            PropertyId = request.PropertyId,
            UnitId = request.UnitId,
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Open" : request.Status.Trim(),
            AssignedTo = request.AssignedTo?.Trim(),
            EstimatedCost = request.EstimatedCost,
            RequestDate = request.RequestDate ?? DateTime.UtcNow,
            CompletedDate = request.CompletedDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.MaintenanceRequests.Add(maintenance);
        await _context.SaveChangesAsync();

        return StatusCode(201, ApiMap.Maintenance(maintenance));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MaintenanceRequestDto request)
    {
        var maintenance = await _context.MaintenanceRequests.FindAsync(id);
        if (maintenance == null)
        {
            return NotFound();
        }

        maintenance.Title = string.IsNullOrWhiteSpace(request.Title) ? maintenance.Title : request.Title.Trim();
        maintenance.Description = request.Description?.Trim() ?? maintenance.Description;
        maintenance.PropertyId = request.PropertyId ?? maintenance.PropertyId;
        maintenance.UnitId = request.UnitId ?? maintenance.UnitId;
        maintenance.Priority = string.IsNullOrWhiteSpace(request.Priority) ? maintenance.Priority : request.Priority.Trim();
        maintenance.Status = string.IsNullOrWhiteSpace(request.Status) ? maintenance.Status : request.Status.Trim();
        maintenance.AssignedTo = request.AssignedTo ?? maintenance.AssignedTo;
        maintenance.EstimatedCost = request.EstimatedCost ?? maintenance.EstimatedCost;
        maintenance.RequestDate = request.RequestDate ?? maintenance.RequestDate;
        maintenance.CompletedDate = request.CompletedDate ?? maintenance.CompletedDate;
        maintenance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiMap.Maintenance(maintenance));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var maintenance = await _context.MaintenanceRequests.FindAsync(id);
        if (maintenance == null)
        {
            return NotFound();
        }

        _context.MaintenanceRequests.Remove(maintenance);
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }
}
