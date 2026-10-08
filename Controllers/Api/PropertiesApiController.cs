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
[Route("api/properties")]
[Authorize]
public class PropertiesApiController : ControllerBase
{
    private readonly RentalContext _context;

    public PropertiesApiController(RentalContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _context.Properties.OrderBy(p => p.Name).ToListAsync();
        return Ok(items.Select(ApiMap.Property));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var property = await _context.Properties.FindAsync(id);
        return property == null ? NotFound() : Ok(ApiMap.Property(property));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PropertyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Address))
        {
            return BadRequest(new { message = "Name and address are required." });
        }

        var property = new Property
        {
            Name = request.Name.Trim(),
            Address = request.Address.Trim(),
            City = request.City?.Trim(),
            State = request.State?.Trim(),
            ZipCode = request.ZipCode?.Trim(),
            Country = request.Country?.Trim(),
            PropertyType = request.PropertyType?.Trim(),
            Units = request.TotalUnits ?? 0,
            PurchasePrice = request.PurchasePrice,
            MarketValue = request.MarketValue,
            Notes = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        return StatusCode(201, ApiMap.Property(property));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PropertyRequest request)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            property.Name = request.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Address))
        {
            property.Address = request.Address.Trim();
        }

        property.City = request.City?.Trim() ?? property.City;
        property.State = request.State?.Trim() ?? property.State;
        property.ZipCode = request.ZipCode?.Trim() ?? property.ZipCode;
        property.Country = request.Country?.Trim() ?? property.Country;
        property.PropertyType = request.PropertyType?.Trim() ?? property.PropertyType;
        property.Units = request.TotalUnits ?? property.Units;
        property.PurchasePrice = request.PurchasePrice ?? property.PurchasePrice;
        property.MarketValue = request.MarketValue ?? property.MarketValue;
        property.Notes = request.Description ?? property.Notes;
        property.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiMap.Property(property));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property == null)
        {
            return NotFound();
        }

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }
}
