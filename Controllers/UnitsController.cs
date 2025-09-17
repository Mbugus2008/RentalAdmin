using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.Models;
using Rental.ViewModels;

namespace Rental.Controllers;

public class UnitsController : Controller
{
    private readonly RentalContext _context;

    public UnitsController(RentalContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? propertyId)
    {
        var unitsQuery = _context.Units
            .Include(u => u.Property)
            .AsNoTracking();

        if (propertyId.HasValue)
        {
            unitsQuery = unitsQuery.Where(u => u.PropertyId == propertyId.Value);
        }

        var units = await unitsQuery
            .OrderBy(u => u.Property!.Name)
            .ThenBy(u => u.UnitType)
            .ToListAsync();

        var properties = await _context.Properties
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();

        var viewModel = new UnitIndexViewModel
        {
            Units = units,
            Properties = properties,
            SelectedPropertyId = propertyId
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var unit = await _context.Units
            .Include(u => u.Property)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (unit == null)
        {
            return NotFound();
        }

        return View(unit);
    }

    public async Task<IActionResult> Create(int? propertyId)
    {
        var properties = await GetPropertiesAsync();

        int? selectedPropertyId = null;
        if (properties.Count > 0)
        {
            selectedPropertyId = propertyId.HasValue && properties.Any(p => p.Id == propertyId.Value)
                ? propertyId.Value
                : properties.First().Id;
        }

        PopulatePropertyOptions(properties, selectedPropertyId);

        var unit = new Unit();
        if (selectedPropertyId.HasValue)
        {
            unit.PropertyId = selectedPropertyId.Value;
        }

        return View(unit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Unit unit)
    {
        if (!await _context.Properties.AnyAsync(p => p.Id == unit.PropertyId))
        {
            ModelState.AddModelError(nameof(Unit.PropertyId), "Please select a valid property.");
        }

        if (!ModelState.IsValid)
        {
            var properties = await GetPropertiesAsync();
            PopulatePropertyOptions(properties, unit.PropertyId);
            return View(unit);
        }

        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = unit.Id });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var unit = await _context.Units
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (unit == null)
        {
            return NotFound();
        }

        var properties = await GetPropertiesAsync();
        PopulatePropertyOptions(properties, unit.PropertyId);

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

        if (!await _context.Properties.AnyAsync(p => p.Id == unit.PropertyId))
        {
            ModelState.AddModelError(nameof(Unit.PropertyId), "Please select a valid property.");
        }

        if (!ModelState.IsValid)
        {
            var properties = await GetPropertiesAsync();
            PopulatePropertyOptions(properties, unit.PropertyId);
            return View(unit);
        }

        var existingUnit = await _context.Units.FirstOrDefaultAsync(u => u.Id == id);
        if (existingUnit == null)
        {
            return NotFound();
        }

        existingUnit.PropertyId = unit.PropertyId;
        existingUnit.UnitType = unit.UnitType;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = existingUnit.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, bool redirectToUnitsIndex = false)
    {
        var unit = await _context.Units.FindAsync(id);
        if (unit == null)
        {
            return redirectToUnitsIndex
                ? RedirectToAction(nameof(Index))
                : RedirectToAction(nameof(PropertiesController.Index), "Properties");
        }

        var propertyId = unit.PropertyId;
        _context.Units.Remove(unit);
        await _context.SaveChangesAsync();

        if (redirectToUnitsIndex)
        {
            return RedirectToAction(nameof(Index), new { propertyId });
        }

        return RedirectToAction(nameof(PropertiesController.Details), "Properties", new { id = propertyId });
    }

    private async Task<List<Property>> GetPropertiesAsync()
    {
        return await _context.Properties
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    private void PopulatePropertyOptions(IEnumerable<Property> properties, int? selectedPropertyId)
    {
        ViewData["PropertyOptions"] = new SelectList(properties, nameof(Property.Id), nameof(Property.Name), selectedPropertyId);
        ViewData["HasProperties"] = properties.Any();
    }
}
