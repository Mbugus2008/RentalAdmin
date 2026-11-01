using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rental.Data;
using Rental.Models;
using Rental.Services;
using Rental.ViewModels;

namespace Rental.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize(Roles = Rental.Services.Roles.AdminArea)]
public class UsersController : Controller
{
    private readonly RentalContext _context;

    public UsersController(RentalContext context)
    {
        _context = context;
    }

    private int CurrentUserId =>
        int.TryParse(User.FindFirst("uid")?.Value, out var id) ? id : 0;

    public async Task<IActionResult> Index()
    {
        var users = await _context.Users
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();
        ViewBag.CurrentUserId = CurrentUserId;
        return View(users);
    }

    public IActionResult Create()
    {
        ViewBag.Roles = new SelectList(Roles.All);
        return View(new UserFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        var email = model.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "A password is required for new users.");
        }

        if (!Roles.IsValid(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Choose a valid user type.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Roles = new SelectList(Roles.All, model.Role);
            return View(model);
        }

        _context.Users.Add(new User
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = email,
            PhoneNumber = model.PhoneNumber?.Trim(),
            Role = Roles.Normalize(model.Role),
            PasswordHash = PasswordHasher.Hash(model.Password!),
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        TempData["UsersSaved"] = $"{model.FirstName} {model.LastName} can now sign in as {Roles.Normalize(model.Role)}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        ViewBag.Roles = new SelectList(Roles.All, Roles.Normalize(user.Role));
        return View(new UserFormViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = Roles.Normalize(user.Role)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserFormViewModel model)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var email = model.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != id))
        {
            ModelState.AddModelError(nameof(model.Email), "Another account already uses this email.");
        }

        if (!Roles.IsValid(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Choose a valid user type.");
        }

        if (!string.IsNullOrWhiteSpace(model.Password) &&
            !string.Equals(model.Password, model.ConfirmPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(model.ConfirmPassword), "The passwords do not match.");
        }

        // Never leave the system without an administrator.
        var keepsAdmin = string.Equals(Roles.Normalize(model.Role), Roles.Admin, StringComparison.OrdinalIgnoreCase);
        var wasAdmin = string.Equals(Roles.Normalize(user.Role), Roles.Admin, StringComparison.OrdinalIgnoreCase);
        if (wasAdmin && !keepsAdmin && await IsLastAdminAsync(user.Id))
        {
            ModelState.AddModelError(nameof(model.Role), "You can't remove the last administrator.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Roles = new SelectList(Roles.All, model.Role);
            return View(model);
        }

        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        user.Email = email;
        user.PhoneNumber = model.PhoneNumber?.Trim();
        user.Role = Roles.Normalize(model.Role);
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = PasswordHasher.Hash(model.Password);
        }

        await _context.SaveChangesAsync();
        TempData["UsersSaved"] = $"{user.FirstName} {user.LastName} updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (user.Id == CurrentUserId)
        {
            TempData["UsersError"] = "You can't delete the account you're signed in with.";
            return RedirectToAction(nameof(Index));
        }

        if (await IsLastAdminAsync(user.Id))
        {
            TempData["UsersError"] = "You can't delete the last administrator.";
            return RedirectToAction(nameof(Index));
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        TempData["UsersSaved"] = $"{user.FirstName} {user.LastName} removed.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> IsLastAdminAsync(int userId)
    {
        var target = await _context.Users.FindAsync(userId);
        if (target == null)
        {
            return false;
        }

        if (!string.Equals(Roles.Normalize(target.Role), Roles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var all = await _context.Users.ToListAsync();
        return all.Count(u => string.Equals(Roles.Normalize(u.Role), Roles.Admin, StringComparison.OrdinalIgnoreCase)) <= 1;
    }
}
