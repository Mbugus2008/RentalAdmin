using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.Models;

namespace Rental.Controllers;

public class PaymentsController : Controller
{
    private readonly RentalContext _context;

    public PaymentsController(RentalContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var payments = await _context.Payments
            .Include(p => p.Lease)
                .ThenInclude(l => l.Property)
            .Include(p => p.Lease)
                .ThenInclude(l => l.Tenant)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
        return View(payments);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateLeasesAsync();
        return View(new Payment { PaymentDate = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Payment payment)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLeasesAsync(payment.LeaseId);
            return View(payment);
        }

        _context.Add(payment);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        await PopulateLeasesAsync(payment.LeaseId);
        return View(payment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Payment payment)
    {
        if (id != payment.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateLeasesAsync(payment.LeaseId);
            return View(payment);
        }

        try
        {
            _context.Update(payment);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Payments.AnyAsync(e => e.Id == payment.Id))
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
        var payment = await _context.Payments.FindAsync(id);
        if (payment != null)
        {
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLeasesAsync(int? selectedLeaseId = null)
    {
        var leases = await _context.Leases
            .Include(l => l.Property)
            .Include(l => l.Tenant)
            .OrderBy(l => l.Property!.Name)
            .ThenBy(l => l.Tenant!.LastName)
            .Select(l => new
            {
                l.Id,
                Description = $"{l.Property!.Name} - {l.Tenant!.FullName}"
            })
            .ToListAsync();

        ViewData["LeaseId"] = new SelectList(leases, "Id", "Description", selectedLeaseId);
    }
}
