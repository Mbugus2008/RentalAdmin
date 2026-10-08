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
[Route("api/documents")]
[Authorize]
public class DocumentsApiController : ControllerBase
{
    private readonly RentalContext _context;

    public DocumentsApiController(RentalContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _context.Documents
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
        return Ok(items.Select(ApiMap.Document));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var document = await _context.Documents.FindAsync(id);
        return document == null ? NotFound() : Ok(ApiMap.Document(document));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "Title is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return BadRequest(new { message = "Category is required." });
        }

        var document = new RentalDocument
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Category = request.Category.Trim(),
            PropertyId = request.PropertyId,
            UnitId = request.UnitId,
            TenantId = request.TenantId,
            ExpirationDate = request.ExpirationDate,
            UploadDate = request.UploadDate ?? DateTime.UtcNow,
            FileType = string.IsNullOrWhiteSpace(request.FileType) ? "PDF" : request.FileType.Trim(),
            FileSize = request.FileSize ?? 0,
            FilePath = request.FilePath ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return StatusCode(201, ApiMap.Document(document));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DocumentRequest request)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        document.Title = string.IsNullOrWhiteSpace(request.Title) ? document.Title : request.Title.Trim();
        document.Description = request.Description ?? document.Description;
        document.Category = string.IsNullOrWhiteSpace(request.Category) ? document.Category : request.Category.Trim();
        document.PropertyId = request.PropertyId ?? document.PropertyId;
        document.UnitId = request.UnitId ?? document.UnitId;
        document.TenantId = request.TenantId ?? document.TenantId;
        document.ExpirationDate = request.ExpirationDate ?? document.ExpirationDate;
        document.UploadDate = request.UploadDate ?? document.UploadDate;
        document.FileType = string.IsNullOrWhiteSpace(request.FileType) ? document.FileType : request.FileType.Trim();
        document.FileSize = request.FileSize ?? document.FileSize;
        document.FilePath = request.FilePath ?? document.FilePath;
        document.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(ApiMap.Document(document));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();
        return Ok(new { success = true });
    }
}
