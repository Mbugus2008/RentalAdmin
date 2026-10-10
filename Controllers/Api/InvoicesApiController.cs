using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental.Api;
using Rental.Data;
using Rental.Models;

namespace Rental.Controllers.Api;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoicesApiController : ControllerBase
{
    private readonly RentalContext _context;

    public InvoicesApiController(RentalContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var invoices = await _context.Invoices
            .Include(i => i.Lines)
            .OrderByDescending(i => i.InvoiceDate)
            .ThenByDescending(i => i.Id)
            .ToListAsync();
        return Ok(invoices.Select(ApiMap.Invoice));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);
        return invoice == null ? NotFound() : Ok(ApiMap.Invoice(invoice));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InvoiceRequest request)
    {
        if (request.TenantId == null)
        {
            return BadRequest(new { message = "Tenant is required." });
        }

        var lines = (request.Lines ?? new List<InvoiceLineRequest>())
            .Where(l => !string.IsNullOrWhiteSpace(l.Description))
            .Select(l => new InvoiceLine
            {
                Description = l.Description!.Trim(),
                Amount = l.Amount ?? 0m
            })
            .ToList();

        if (lines.Count == 0)
        {
            return BadRequest(new { message = "At least one invoice line is required." });
        }

        var invoiceDate = request.InvoiceDate ?? DateTime.Today;
        var invoice = new Invoice
        {
            TenantId = request.TenantId.Value,
            PropertyId = request.PropertyId,
            UnitId = request.UnitId,
            InvoiceDate = invoiceDate,
            DueDate = request.DueDate ?? invoiceDate.AddDays(5),
            AmountPaid = request.AmountPaid ?? 0m,
            Notes = request.Notes,
            Lines = lines,
            TotalAmount = lines.Sum(l => l.Amount),
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Sent" : request.Status!.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        invoice.Status = ComputeStatus(invoice.Status, invoice.TotalAmount, invoice.AmountPaid);

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Assign a human-friendly number based on the generated id.
        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            invoice.InvoiceNumber = $"INV-{invoice.Id:D4}";
            await _context.SaveChangesAsync();
        }

        return StatusCode(201, ApiMap.Invoice(invoice));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] InvoiceRequest request)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null)
        {
            return NotFound();
        }

        if (request.TenantId != null) invoice.TenantId = request.TenantId.Value;
        if (request.PropertyId != null) invoice.PropertyId = request.PropertyId;
        if (request.UnitId != null) invoice.UnitId = request.UnitId;
        if (request.InvoiceDate != null) invoice.InvoiceDate = request.InvoiceDate.Value;
        if (request.DueDate != null) invoice.DueDate = request.DueDate;
        if (request.AmountPaid != null) invoice.AmountPaid = request.AmountPaid.Value;
        if (request.Notes != null) invoice.Notes = request.Notes;

        if (request.Lines != null)
        {
            _context.InvoiceLines.RemoveRange(invoice.Lines);
            invoice.Lines = request.Lines
                .Where(l => !string.IsNullOrWhiteSpace(l.Description))
                .Select(l => new InvoiceLine
                {
                    Description = l.Description!.Trim(),
                    Amount = l.Amount ?? 0m
                })
                .ToList();
            invoice.TotalAmount = invoice.Lines.Sum(l => l.Amount);
        }

        invoice.Status = ComputeStatus(
            request.Status?.Trim() ?? invoice.Status,
            invoice.TotalAmount,
            invoice.AmountPaid);
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiMap.Invoice(invoice));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null)
        {
            return NotFound();
        }

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }

    private static string ComputeStatus(string current, decimal total, decimal paid)
    {
        if (paid <= 0)
        {
            return current == "Draft" ? "Draft" : "Sent";
        }

        return paid >= total ? "Paid" : "Partial";
    }
}
