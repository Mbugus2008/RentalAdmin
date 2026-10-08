using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rental.Api;
using Rental.Data;

namespace Rental.Controllers.Api;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersApiController : ControllerBase
{
    private readonly RentalContext _context;

    public UsersApiController(RentalContext context)
    {
        _context = context;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
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

        return Ok(ApiMap.User(user));
    }
}
