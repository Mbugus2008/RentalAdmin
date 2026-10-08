using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rental.Api;
using Rental.Data;
using Rental.Models;
using Rental.Services;

namespace Rental.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly RentalContext _context;
    private readonly JwtService _jwt;

    public AuthApiController(RentalContext context, JwtService jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(new { token = _jwt.GenerateToken(user) });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Password)
            || string.IsNullOrWhiteSpace(request.FirstName)
            || string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(new { message = "First name, last name, email and password are required." });
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            return BadRequest(new { message = "An account with this email already exists." });
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PhoneNumber = (request.PhoneNumber ?? request.Phone)?.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = "Landlord",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Registration successful.", user = ApiMap.User(user) });
    }

    [HttpPost("oauth/{provider}")]
    [AllowAnonymous]
    public IActionResult OAuth(string provider)
    {
        return StatusCode(501, new { message = $"OAuth provider '{provider}' is not configured on this server." });
    }

    [HttpGet("validate")]
    [Authorize]
    public IActionResult Validate()
    {
        return Ok(new { valid = true });
    }

    [HttpPost("refresh")]
    [Authorize]
    public async Task<IActionResult> Refresh()
    {
        var uid = User.FindFirst("uid")?.Value;
        if (!int.TryParse(uid, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(new { token = _jwt.GenerateToken(user) });
    }
}
