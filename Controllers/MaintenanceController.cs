using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.Models;

namespace Rental.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = Rental.Services.Roles.MaintenanceArea)]
public class MaintenanceController : Controller
{
    private readonly RentalContext _context;

    public MaintenanceController(RentalContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var requests = await _context.MaintenanceRequests
            .Include(m => m.Property)
            .OrderByDescending(m => m.RequestDate)
            .ThenByDescending(m => m.Id)
            .ToListAsync();

        ViewBag.OpenCount = requests.Count(r =>
            string.Equals(r.Status, Rental.Services.MaintenanceStatus.Open, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r.Status, Rental.Services.MaintenanceStatus.Assigned, StringComparison.OrdinalIgnoreCase));
        ViewBag.InProgressCount = requests.Count(r =>
            string.Equals(r.Status, Rental.Services.MaintenanceStatus.InProgress, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r.Status, Rental.Services.MaintenanceStatus.Pending, StringComparison.OrdinalIgnoreCase));
        ViewBag.CompletedCount = requests.Count(r =>
            string.Equals(r.Status, Rental.Services.MaintenanceStatus.Completed, StringComparison.OrdinalIgnoreCase));

        return View(requests);
    }

    public async Task<IActionResult> Create()
    {
        await LoadPropertiesAsync();
        return View(new MaintenanceRequest
        {
            RequestDate = DateTime.Today,
            Status = "Open",
            Priority = "Medium"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MaintenanceRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadPropertiesAsync(request.PropertyId);
            return View(request);
        }

        request.CreatedAt = DateTime.UtcNow;
        _context.Add(request);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var request = await _context.MaintenanceRequests.FindAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        await LoadPropertiesAsync(request.PropertyId);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MaintenanceRequest request)
    {
        if (id != request.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadPropertiesAsync(request.PropertyId);
            return View(request);
        }

        request.UpdatedAt = DateTime.UtcNow;
        _context.Update(request);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var request = await _context.MaintenanceRequests.FindAsync(id);
        if (request != null)
        {
            _context.MaintenanceRequests.Remove(request);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadPropertiesAsync(int? propertyId = null)
    {
        var properties = await _context.Properties.OrderBy(p => p.Name).ToListAsync();
        ViewData["PropertyId"] = new SelectList(properties, "Id", "Name", propertyId);
    }
}
