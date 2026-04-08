using System.Security.Claims;
using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/profile")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "User")]
    [HttpPost]
    public async Task<IActionResult> Profile([FromBody] ProfileRequest request)
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

       var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new
            {
                status = 401,
                message = "Invalid user",
                data = (object?)null
            });
        }

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return Unauthorized(new
            {
                status = 401,
                message = "User does not exist",
                data = (object?)null
            });
        }
        user.FullName = request.fullName;
        user.IsUniversityStudent = request.IsUniversityStudent;

      await _db.SaveChangesAsync();

        return Ok(new
        {
          status= 200,
          message= "Profile Updated succesfully"  ,
          data = new ProfileResponse
          {
            FullName = user.FullName,
            MobileNumber = user.MobileNumber!,
            IsUniversityStudent = user.IsUniversityStudent,
            WalletBalance = user.WalletBalance

          }
        });

     }

      
}

public class ProfileRequest{
  public required string ApiKey { get; set; }
  public required string fullName{get; set;}

  public bool IsUniversityStudent{get; set;}

}
public class ProfileResponse{
  public required string FullName { get; set; }

  public required string MobileNumber{get; set;}

  public bool IsUniversityStudent{get; set;}

   public decimal WalletBalance{get; set;}

}

