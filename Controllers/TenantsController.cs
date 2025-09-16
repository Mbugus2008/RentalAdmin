using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.Models;

namespace Rental.Controllers;

public class TenantsController : Controller
{
    private readonly RentalContext _context;

    public TenantsController(RentalContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var tenants = await _context.Tenants
            .Include(t => t.Leases)
                .ThenInclude(l => l.Property)
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToListAsync();
        return View(tenants);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tenant = await _context.Tenants
            .Include(t => t.Leases)
                .ThenInclude(l => l.Property)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null)
        {
            return NotFound();
        }

        return View(tenant);
    }

    public IActionResult Create()
    {
        return View(new Tenant());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Tenant tenant)
    {
        if (!ModelState.IsValid)
        {
            return View(tenant);
        }

        _context.Add(tenant);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null)
        {
            return NotFound();
        }

        return View(tenant);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Tenant tenant)
    {
        if (id != tenant.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(tenant);
        }

        try
        {
            _context.Update(tenant);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Tenants.AnyAsync(e => e.Id == tenant.Id))
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
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant != null)
        {
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
