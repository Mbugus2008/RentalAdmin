using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.Models;

namespace Rental.Controllers;

public class PropertiesController : Controller
{
    private readonly RentalContext _context;

    public PropertiesController(RentalContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var properties = await _context.Properties
            .Include(p => p.Leases)
            .Include(p => p.Units)
            .AsSplitQuery()
            .OrderBy(p => p.Name)
            .ToListAsync();
        return View(properties);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var property = await _context.Properties
            .Include(p => p.Units)
            .Include(p => p.Leases)
                .ThenInclude(l => l.Tenant)
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == id);
        if (property == null)
        {
            return NotFound();
        }

        return View(property);
    }

    public IActionResult Create()
    {
        return View(new Property());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Property property)
    {
        if (!ModelState.IsValid)
        {
            return View(property);
        }

        _context.Add(property);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var property = await _context.Properties.FindAsync(id);
        if (property == null)
        {
            return NotFound();
        }

        return View(property);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Property property)
    {
        if (id != property.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(property);
        }

        var existingProperty = await _context.Properties.FindAsync(id);
        if (existingProperty == null)
        {
            return NotFound();
        }

        existingProperty.Name = property.Name;
        existingProperty.TotalUnits = property.TotalUnits;
        existingProperty.Address = property.Address;
        existingProperty.City = property.City;
        existingProperty.State = property.State;
        existingProperty.ZipCode = property.ZipCode;
        existingProperty.Notes = property.Notes;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Properties.AnyAsync(e => e.Id == property.Id))
            {
                return NotFound();
            }
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property != null)
        {
            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
