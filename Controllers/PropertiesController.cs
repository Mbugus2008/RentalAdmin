using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.Models;

namespace Rental.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = Rental.Services.Roles.PortfolioArea)]
public class PropertiesController : Controller
{
    private readonly RentalContext _context;

    public PropertiesController(RentalContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var properties = await _context.Properties
            .Include(p => p.Leases)
            .OrderBy(p => p.Name)
            .ToListAsync();

        // Agent-facing status: Maintenance (open requests) > Occupied (active lease) > Vacant.
        var maintenanceIds = (await _context.MaintenanceRequests
                .Select(m => new { m.PropertyId, m.Status })
                .ToListAsync())
            .Where(m => !Rental.Services.MaintenanceStatus.IsClosed(m.Status))
            .Select(m => m.PropertyId ?? 0)
            .ToHashSet();
        var occupiedIds = properties
            .Where(p => p.Leases.Any(l => l.EndDate == null || l.EndDate >= today))
            .Select(p => p.Id)
            .ToHashSet();

        ViewBag.MaintenancePropertyIds = maintenanceIds;
        ViewBag.OccupiedPropertyIds = occupiedIds;
        return View(properties);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var property = await _context.Properties
            .Include(p => p.Leases)
                .ThenInclude(l => l.Tenant)
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

        try
        {
            _context.Update(property);
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
