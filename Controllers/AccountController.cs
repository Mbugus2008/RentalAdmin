using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Rental.Data;
using Rental.Services;
using Rental.ViewModels;

namespace Rental.Controllers;

/// <summary>Sign-in, sign-out and password management for the admin console.</summary>
public class AccountController : Controller
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutWindow = TimeSpan.FromMinutes(5);

    // Verified against when the account does not exist so failed lookups take
    // roughly the same time as wrong passwords (avoids user-enumeration timing).
    private static readonly string DummyHash = PasswordHasher.Hash("timing-equalization-dummy");

    private readonly RentalContext _context;
    private readonly IMemoryCache _cache;

    public AccountController(RentalContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return HomeForRole(User.FindFirst(ClaimTypes.Role)?.Value);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim().ToLowerInvariant();
        var attemptKey = $"login-fail:{email}:{HttpContext.Connection.RemoteIpAddress}";

        if (_cache.TryGetValue(attemptKey, out int failures) && failures >= MaxFailedAttempts)
        {
            ModelState.AddModelError(string.Empty,
                $"Too many failed attempts. Please wait {LockoutWindow.TotalMinutes:0} minutes and try again.");
            return View(model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        var passwordOk = PasswordHasher.Verify(model.Password, user?.PasswordHash ?? DummyHash);

        if (user is null || !passwordOk)
        {
            _cache.Set(attemptKey, failures + 1, LockoutWindow);
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        _cache.Remove(attemptKey);

        var claims = new List<Claim>
        {
            new("uid", user.Id.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, Roles.Normalize(user.Role))
        };

        var properties = new AuthenticationProperties { IsPersistent = model.RememberMe };
        if (model.RememberMe)
        {
            properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            properties);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return HomeForRole(Roles.Normalize(user.Role));
    }

    /// <summary>Maintenance staff land straight on their job list; everyone else on the dashboard.</summary>
    private IActionResult HomeForRole(string? role) =>
        string.Equals(Roles.Normalize(role), Roles.Maintenance, StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Index", "Maintenance")
            : RedirectToAction("Index", "Home");

    [Authorize]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!int.TryParse(User.FindFirst("uid")?.Value, out var userId))
        {
            return RedirectToAction(nameof(Login));
        }

        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!PasswordHasher.Verify(model.CurrentPassword, user.PasswordHash))
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "The current password is incorrect.");
            return View(model);
        }

        if (PasswordHasher.Verify(model.NewPassword, user.PasswordHash))
        {
            ModelState.AddModelError(nameof(model.NewPassword), "The new password must be different from the current one.");
            return View(model);
        }

        user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
        await _context.SaveChangesAsync();

        TempData["PasswordChanged"] = true;
        return RedirectToAction(nameof(ChangePassword));
    }
}
