using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental.Api;
using Rental.Data;
using Rental.Models;

namespace Rental.Controllers.Api;

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsApiController : ControllerBase
{
    private readonly RentalContext _context;

    public SettingsApiController(RentalContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new AppSettings();
            _context.AppSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return Ok(ApiMap.Settings(settings));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] SettingsRequest request)
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new AppSettings();
            _context.AppSettings.Add(settings);
        }

        if (request.WaterRatePerUnit != null)
        {
            settings.WaterRatePerUnit = request.WaterRatePerUnit.Value;
        }
        if (request.GarbageFee != null)
        {
            settings.GarbageFee = request.GarbageFee.Value;
        }
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiMap.Settings(settings));
    }
}
