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
[Route("api/transactions")]
[Authorize]
public class TransactionsApiController : ControllerBase
{
    private readonly RentalContext _context;

    public TransactionsApiController(RentalContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _context.Transactions
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
        return Ok(items.Select(ApiMap.Transaction));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        return transaction == null ? NotFound() : Ok(ApiMap.Transaction(transaction));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TransactionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return BadRequest(new { message = "Category is required." });
        }

        if (request.Amount == null)
        {
            return BadRequest(new { message = "Amount is required." });
        }

        var type = string.IsNullOrWhiteSpace(request.Type) ? "Expense" : request.Type.Trim();

        var transaction = new Transaction
        {
            TransactionDate = request.TransactionDate ?? DateTime.UtcNow,
            Amount = request.Amount.Value,
            Category = request.Category.Trim(),
            Type = type,
            PropertyId = request.PropertyId,
            UnitId = request.UnitId,
            TenantId = request.TenantId,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return StatusCode(201, ApiMap.Transaction(transaction));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TransactionRequest request)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null)
        {
            return NotFound();
        }

        transaction.TransactionDate = request.TransactionDate ?? transaction.TransactionDate;
        transaction.Amount = request.Amount ?? transaction.Amount;
        transaction.Category = string.IsNullOrWhiteSpace(request.Category) ? transaction.Category : request.Category.Trim();
        transaction.Type = string.IsNullOrWhiteSpace(request.Type) ? transaction.Type : request.Type.Trim();
        transaction.PropertyId = request.PropertyId ?? transaction.PropertyId;
        transaction.UnitId = request.UnitId ?? transaction.UnitId;
        transaction.TenantId = request.TenantId ?? transaction.TenantId;
        transaction.Description = request.Description ?? transaction.Description;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiMap.Transaction(transaction));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null)
        {
            return NotFound();
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }
}
