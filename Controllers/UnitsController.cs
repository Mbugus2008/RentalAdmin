using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.Models;

namespace Rental.Controllers;

public class UnitsController : Controller
{
    private readonly RentalContext _context;

    public UnitsController(RentalContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Create(int propertyId)
    {
        var property = await _context.Properties.FindAsync(propertyId);
        if (property == null)
        {
            return NotFound();
        }

        ViewData["PropertyName"] = property.Name;
        return View(new Unit { PropertyId = propertyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Unit unit)
    {
        var property = await _context.Properties.FindAsync(unit.PropertyId);
        if (property == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewData["PropertyName"] = property.Name;
            return View(unit);
        }

        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(PropertiesController.Details), "Properties", new { id = unit.PropertyId });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var unit = await _context.Units
            .Include(u => u.Property)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (unit == null)
        {
            return NotFound();
        }

        ViewData["PropertyName"] = unit.Property?.Name;
        return View(unit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Unit unit)
    {
        if (id != unit.Id)
        {
            return NotFound();
        }

        var property = await _context.Properties.FindAsync(unit.PropertyId);
        if (property == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewData["PropertyName"] = property.Name;
            return View(unit);
        }

        var existingUnit = await _context.Units.FirstOrDefaultAsync(u => u.Id == id);
        if (existingUnit == null)
        {
            return NotFound();
        }

        existingUnit.UnitNumber = unit.UnitNumber;
        existingUnit.UnitType = unit.UnitType;
        existingUnit.Notes = unit.Notes;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(PropertiesController.Details), "Properties", new { id = unit.PropertyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var unit = await _context.Units.FindAsync(id);
        if (unit == null)
        {
            return RedirectToAction(nameof(PropertiesController.Index), "Properties");
        }

        var propertyId = unit.PropertyId;
        _context.Units.Remove(unit);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(PropertiesController.Details), "Properties", new { id = propertyId });
    }
}
