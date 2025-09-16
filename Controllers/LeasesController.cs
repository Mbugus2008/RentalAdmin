using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.Models;

namespace Rental.Controllers;

public class LeasesController : Controller
{
    private readonly RentalContext _context;

    public LeasesController(RentalContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var leases = await _context.Leases
            .Include(l => l.Property)
            .Include(l => l.Tenant)
            .OrderBy(l => l.Property!.Name)
            .ThenBy(l => l.Tenant!.LastName)
            .ToListAsync();
        return View(leases);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var lease = await _context.Leases
            .Include(l => l.Property)
            .Include(l => l.Tenant)
            .Include(l => l.Payments)
            .FirstOrDefaultAsync(l => l.Id == id);
        if (lease == null)
        {
            return NotFound();
        }

        return View(lease);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View(new Lease { StartDate = System.DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Lease lease)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(lease.PropertyId, lease.TenantId);
            return View(lease);
        }

        _context.Add(lease);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var lease = await _context.Leases.FindAsync(id);
        if (lease == null)
        {
            return NotFound();
        }

        await PopulateLookupsAsync(lease.PropertyId, lease.TenantId);
        return View(lease);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Lease lease)
    {
        if (id != lease.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(lease.PropertyId, lease.TenantId);
            return View(lease);
        }

        try
        {
            _context.Update(lease);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Leases.AnyAsync(e => e.Id == lease.Id))
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
        var lease = await _context.Leases.FindAsync(id);
        if (lease != null)
        {
            _context.Leases.Remove(lease);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(int? selectedPropertyId = null, int? selectedTenantId = null)
    {
        var properties = await _context.Properties
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name })
            .ToListAsync();
        var tenants = await _context.Tenants
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .Select(t => new { t.Id, Name = t.FullName })
            .ToListAsync();

        ViewData["PropertyId"] = new SelectList(properties, "Id", "Name", selectedPropertyId);
        ViewData["TenantId"] = new SelectList(tenants, "Id", "Name", selectedTenantId);
    }
}
