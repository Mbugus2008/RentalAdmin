using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.Models;

namespace Rental.Controllers;

public class TransactionsController : Controller
{
    private readonly RentalContext _context;

    public TransactionsController(RentalContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var transactions = await _context.Transactions
            .Include(t => t.Property)
            .Include(t => t.Tenant)
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        ViewBag.TotalIncome = transactions
            .Where(t => string.Equals(t.Type, "Income", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);
        ViewBag.TotalExpense = transactions
            .Where(t => !string.Equals(t.Type, "Income", StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);

        return View(transactions);
    }

    public async Task<IActionResult> Create()
    {
        await LoadListsAsync();
        return View(new Transaction
        {
            TransactionDate = DateTime.Today,
            Type = "Expense"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Transaction transaction)
    {
        if (!ModelState.IsValid)
        {
            await LoadListsAsync(transaction.PropertyId, transaction.TenantId);
            return View(transaction);
        }

        transaction.CreatedAt = DateTime.UtcNow;
        _context.Add(transaction);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null)
        {
            return NotFound();
        }

        await LoadListsAsync(transaction.PropertyId, transaction.TenantId);
        return View(transaction);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Transaction transaction)
    {
        if (id != transaction.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadListsAsync(transaction.PropertyId, transaction.TenantId);
            return View(transaction);
        }

        transaction.UpdatedAt = DateTime.UtcNow;
        _context.Update(transaction);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadListsAsync(int? propertyId = null, int? tenantId = null)
    {
        var properties = await _context.Properties.OrderBy(p => p.Name).ToListAsync();
        var tenants = await _context.Tenants.OrderBy(t => t.LastName).ToListAsync();
        ViewData["PropertyId"] = new SelectList(properties, "Id", "Name", propertyId);
        ViewData["TenantId"] = new SelectList(tenants, "Id", "FullName", tenantId);
    }
}
