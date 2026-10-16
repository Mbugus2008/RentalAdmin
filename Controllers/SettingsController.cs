using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.Models;

namespace Rental.Controllers;

public class SettingsController : Controller
{
    private readonly RentalContext _context;

    public SettingsController(RentalContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await GetOrCreateAsync();
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AppSettings form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var settings = await GetOrCreateAsync();
        settings.WaterRatePerUnit = form.WaterRatePerUnit;
        settings.GarbageFee = form.GarbageFee;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["SettingsSaved"] = true;
        return RedirectToAction(nameof(Index));
    }

    private async Task<AppSettings> GetOrCreateAsync()
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new AppSettings();
            _context.AppSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return settings;
    }
}
