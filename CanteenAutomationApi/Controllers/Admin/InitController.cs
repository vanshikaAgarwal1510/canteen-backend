using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost]
    public IActionResult GetLoggedInUser(InitRequest request)
    {
       if (request.ApiKey != Constants.api)
        {
            return Unauthorized(new
            {
                status = 401,
                message = "An invalid API key was provided",
                data = (object?)null
            });
        }
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var emailClaim = User.FindFirst(ClaimTypes.Email);
        var roleClaim = User.FindFirst(ClaimTypes.Role);

        if (userIdClaim == null || emailClaim == null || roleClaim == null)
        {
             return Unauthorized(new
         {
             status = 401,
             message = "Invalid auth token",
             data = (object?)null
         });
        }

        var userProfile = new UserProfileDto
        {
            UserId = int.Parse(userIdClaim.Value),
            Email = emailClaim.Value,
            Role = roleClaim.Value
        };

        return Ok(userProfile);
    }
}
class UserProfileDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
public class InitRequest
{
    public string ApiKey { get; set; } = string.Empty;
}