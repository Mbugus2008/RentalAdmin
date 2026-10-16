using System;
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

public class InvoicesController : Controller
{
    private readonly RentalContext _context;

    public InvoicesController(RentalContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var invoices = await _context.Invoices
            .Include(i => i.Tenant)
            .OrderByDescending(i => i.InvoiceDate)
            .ThenByDescending(i => i.Id)
            .ToListAsync();

        ViewBag.InvoicedTotal = invoices.Sum(i => i.TotalAmount);
        ViewBag.CollectedTotal = invoices.Sum(i => i.AmountPaid);
        ViewBag.OutstandingTotal = invoices.Sum(i => i.Balance);

        return View(invoices);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var invoice = await _context.Invoices
            .Include(i => i.Tenant)
            .Include(i => i.Property)
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null)
        {
            return NotFound();
        }

        return View(invoice);
    }

    public async Task<IActionResult> Create()
    {
        await LoadTenantsAsync();
        return View(new InvoiceFormViewModel
        {
            InvoiceDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(5)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceFormViewModel form)
    {
        var lines = CleanLines(form);
        if (lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Add at least one invoice line with a description.");
        }

        if (!ModelState.IsValid)
        {
            await LoadTenantsAsync(form.TenantId, form.PropertyId);
            return View(form);
        }

        var invoice = new Invoice
        {
            TenantId = form.TenantId!.Value,
            PropertyId = form.PropertyId,
            InvoiceDate = form.InvoiceDate,
            DueDate = form.DueDate ?? form.InvoiceDate.AddDays(5),
            Notes = form.Notes,
            AmountPaid = form.AmountPaid,
            Lines = lines,
            TotalAmount = lines.Sum(l => l.Amount),
            CreatedAt = DateTime.UtcNow
        };
        invoice.Status = ComputeStatus(invoice.TotalAmount, invoice.AmountPaid, form.Status);

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Assign a human-friendly number based on the generated id (mirrors the API).
        invoice.InvoiceNumber = $"INV-{invoice.Id:D4}";
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var invoice = await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null)
        {
            return NotFound();
        }

        await LoadTenantsAsync(invoice.TenantId, invoice.PropertyId);
        return View(new InvoiceFormViewModel
        {
            Id = invoice.Id,
            TenantId = invoice.TenantId,
            PropertyId = invoice.PropertyId,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Notes = invoice.Notes,
            Status = invoice.Status,
            AmountPaid = invoice.AmountPaid,
            Lines = invoice.Lines.Count > 0
                ? invoice.Lines.Select(l => new InvoiceFormViewModel.InvoiceLineInput
                {
                    Description = l.Description,
                    Amount = l.Amount
                }).ToList()
                : new List<InvoiceFormViewModel.InvoiceLineInput> { new() }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InvoiceFormViewModel form)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null)
        {
            return NotFound();
        }

        var lines = CleanLines(form);
        if (lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Add at least one invoice line with a description.");
        }

        if (!ModelState.IsValid)
        {
            await LoadTenantsAsync(form.TenantId, form.PropertyId);
            return View(form);
        }

        invoice.TenantId = form.TenantId!.Value;
        invoice.PropertyId = form.PropertyId;
        invoice.InvoiceDate = form.InvoiceDate;
        invoice.DueDate = form.DueDate;
        invoice.Notes = form.Notes;
        invoice.AmountPaid = form.AmountPaid;

        _context.InvoiceLines.RemoveRange(invoice.Lines);
        invoice.Lines = lines;
        invoice.TotalAmount = lines.Sum(l => l.Amount);
        invoice.Status = ComputeStatus(invoice.TotalAmount, invoice.AmountPaid, form.Status);
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    public async Task<IActionResult> RecordPayment(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var invoice = await _context.Invoices
            .Include(i => i.Tenant)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null)
        {
            return NotFound();
        }

        return View(new InvoicePaymentViewModel
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            TenantName = invoice.Tenant?.FullName ?? string.Empty,
            Balance = invoice.Balance,
            Amount = invoice.Balance > 0 ? invoice.Balance : 0m
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(InvoicePaymentViewModel form)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Tenant)
            .FirstOrDefaultAsync(i => i.Id == form.InvoiceId);
        if (invoice == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            form.InvoiceNumber = invoice.InvoiceNumber;
            form.TenantName = invoice.Tenant?.FullName ?? string.Empty;
            form.Balance = invoice.Balance;
            return View(form);
        }

        invoice.AmountPaid += form.Amount;
        invoice.Status = ComputeStatus(invoice.TotalAmount, invoice.AmountPaid, invoice.Status);
        invoice.UpdatedAt = DateTime.UtcNow;

        // Keep the ledger in sync: recording a payment also books an income transaction
        // (matches the mobile app behaviour).
        _context.Transactions.Add(new Transaction
        {
            TransactionDate = form.PaymentDate,
            Amount = form.Amount,
            Category = "Rent",
            Type = "Income",
            TenantId = invoice.TenantId,
            PropertyId = invoice.PropertyId,
            Description = $"Payment for {invoice.InvoiceNumber}",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (invoice != null)
        {
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private static List<InvoiceLine> CleanLines(InvoiceFormViewModel form)
    {
        return form.Lines
            .Where(l => !string.IsNullOrWhiteSpace(l.Description))
            .Select(l => new InvoiceLine
            {
                Description = l.Description!.Trim(),
                Amount = l.Amount
            })
            .ToList();
    }

    private static string ComputeStatus(decimal total, decimal paid, string current)
    {
        if (paid <= 0)
        {
            return current == "Draft" ? "Draft" : "Sent";
        }

        return paid >= total ? "Paid" : "Partial";
    }

    private async Task LoadTenantsAsync(int? tenantId = null, int? propertyId = null)
    {
        var tenants = await _context.Tenants.OrderBy(t => t.LastName).ToListAsync();
        var properties = await _context.Properties.OrderBy(p => p.Name).ToListAsync();
        ViewData["TenantId"] = new SelectList(tenants, "Id", "FullName", tenantId);
        ViewData["PropertyId"] = new SelectList(properties, "Id", "Name", propertyId);
    }
}
