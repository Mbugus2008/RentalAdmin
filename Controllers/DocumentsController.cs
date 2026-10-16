using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.Models;

namespace Rental.Controllers;

public class DocumentsController : Controller
{
    private static readonly string[] AllowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx", ".xls", ".xlsx", ".txt" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    private readonly RentalContext _context;
    private readonly IWebHostEnvironment _environment;

    public DocumentsController(RentalContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var documents = await _context.Documents
            .Include(d => d.Property)
            .Include(d => d.Tenant)
            .OrderByDescending(d => d.UploadDate)
            .ThenByDescending(d => d.Id)
            .ToListAsync();
        return View(documents);
    }

    public async Task<IActionResult> Create()
    {
        await LoadListsAsync();
        return View(new RentalDocument
        {
            UploadDate = DateTime.Today,
            Category = "Lease Agreement",
            FileType = "PDF"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RentalDocument document, IFormFile? file)
    {
        ValidateFile(file);

        if (!ModelState.IsValid)
        {
            await LoadListsAsync(document.PropertyId, document.TenantId);
            return View(document);
        }

        var stored = await SaveFileAsync(file!);
        document.FilePath = stored.relativePath;
        document.FileType = stored.fileType;
        document.FileSize = stored.size;
        document.UploadDate = DateTime.Today;
        document.CreatedAt = DateTime.UtcNow;

        _context.Add(document);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var document = await _context.Documents.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        await LoadListsAsync(document.PropertyId, document.TenantId);
        return View(document);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RentalDocument document)
    {
        if (id != document.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadListsAsync(document.PropertyId, document.TenantId);
            return View(document);
        }

        var existing = await _context.Documents.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.Title = document.Title;
        existing.Category = document.Category;
        existing.Description = document.Description;
        existing.PropertyId = document.PropertyId;
        existing.TenantId = document.TenantId;
        existing.ExpirationDate = document.ExpirationDate;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Download(int id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(document.FilePath))
        {
            return NotFound();
        }

        // External URL (uploaded through the mobile API) — send the browser there.
        if (document.FilePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(document.FilePath);
        }

        var physicalPath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (!System.IO.File.Exists(physicalPath))
        {
            return NotFound();
        }

        var extension = Path.GetExtension(physicalPath);
        var downloadName = $"{document.Title}{extension}";
        return PhysicalFile(physicalPath, "application/octet-stream", downloadName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document != null)
        {
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            TryDeletePhysicalFile(document.FilePath);
        }
        return RedirectToAction(nameof(Index));
    }

    private void ValidateFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("file", "Choose a file to upload.");
            return;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            ModelState.AddModelError("file", $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}");
        }

        if (file.Length > MaxFileSize)
        {
            ModelState.AddModelError("file", "File is larger than the 10 MB limit.");
        }
    }

    private async Task<(string relativePath, string fileType, int size)> SaveFileAsync(IFormFile file)
    {
        var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "documents");
        Directory.CreateDirectory(uploadsDir);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsDir, storedName);

        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream);
        }

        return ($"/uploads/documents/{storedName}", extension.TrimStart('.').ToUpperInvariant(), (int)file.Length);
    }

    private void TryDeletePhysicalFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            var physicalPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }
        catch (IOException)
        {
            // The record is gone; a stray file is harmless.
        }
    }

    private async Task LoadListsAsync(int? propertyId = null, int? tenantId = null)
    {
        var properties = await _context.Properties.OrderBy(p => p.Name).ToListAsync();
        var tenants = await _context.Tenants.OrderBy(t => t.LastName).ToListAsync();
        ViewData["PropertyId"] = new SelectList(properties, "Id", "Name", propertyId);
        ViewData["TenantId"] = new SelectList(tenants, "Id", "FullName", tenantId);
    }
}
