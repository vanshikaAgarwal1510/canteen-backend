using System.Security.Claims;
using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

 [Authorize(Roles = "Admin")]
[Route("api/settings")]
[ApiController]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db)
    {
        _db = db;
    }


      [HttpPost("details")]
    public async Task<IActionResult> GetDetails(GetStaffRequest request)
    {
          if (request.ApiKey != Constants.api)
    {
        return Unauthorized(new
        {
            status = 401,
            message = "Invalid API key",
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

       var settings = await _db.Settings.FirstOrDefaultAsync();

    if (settings == null)
    {
        return Ok(new
        {
            status = 200,
            message = "No data found",
            data = (object?)null
        });
    }

       var now = DateTime.Now.TimeOfDay;
       
       bool isWithinTime = now >= settings.OpeningTime &&
                           now <= settings.ClosingTime;
       
       bool isOpen = isWithinTime && !settings.IsOpen;

        return Ok(new
       {
          status = 200,
          message = "Details retrieved successfully",
         data = new DetailsResponse
              {
               CanteenName = settings.CanteenName,
               Address =settings.Address,
               ContactNumber =settings.ContactNumber,
               OpeningTime = settings.OpeningTime,
               ClosingTime = settings.ClosingTime,
               IsOpen =isOpen,
               AccountCreated =user.CreatedAt,
               LastLogin= user.LastLoginAt,
               IsOnlineOrderingEnabled = settings.IsOnlineOrderingEnabled,
               RequirePickupCode = settings.RequirePickupCode,
               MaxActiveOrders =settings.MaxActiveOrders    
              }
      });
    }
   
   
    [HttpPost("profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] AdminProfileRequest request)
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

        user.FullName = request.FullName;

    if (request.MobileNumber != null)
    {
      user.MobileNumber = request.MobileNumber;
    }

      if (request.Image != null)
       
    {

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(request.Image.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                status = 400,
                message = "Invalid image format",
                data = (object?)null
            });
        }

        if (request.Image.Length > 2 * 1024 * 1024)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Image too large",
                data = (object?)null
            });
        }
        // delete old image
        if (!string.IsNullOrEmpty(user.ImageUrl))
        {
            var oldPath = Path.Combine(
                "wwwroot",
                user.ImageUrl.TrimStart('/')
            );

            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }
        

        // save new image
        var fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);
        var folderPath = Path.Combine("wwwroot/uploads/profile");

        Directory.CreateDirectory(folderPath);

        var fullPath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await request.Image.CopyToAsync(stream);

        user.ImageUrl = "/uploads/profile/" + fileName;
    }

        

      await _db.SaveChangesAsync();

        return Ok(new
        {
          status= 200,
          message= "Profile Updated succesfully"  ,
          data = new AdminProfileResponse
          {
            FullName = user.FullName,
            MobileNumber = user.MobileNumber!,
            ImageUrl =user.ImageUrl
          }
        });

     }
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
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

    
      if ( !PasswordHelper.Verify(request.OldPassword, user.PasswordHash!))
     {
         return Unauthorized(new
         {
             status = 401,
             message = "Invalid old password",
             data = (object?)null
         });
     }

      user.PasswordHash = PasswordHelper.Hash(request.NewPassword);


      await _db.SaveChangesAsync();

        return Ok(new
        {
          status= 200,
          message= "Changed Password succesfully",
          data = (object?)null
        });
     }

    [HttpPost("update-canteen-detail")]
    public async Task<IActionResult> UpdateCanteenDetail(CanteenDetailRequest request)
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

            
       var settings = await _db.Settings.FirstOrDefaultAsync();

    if (settings == null)
    {
        return Ok(new
        {
            status = 200,
            message = "No data found",
            data = (object?)null
        });
    }
    settings.CanteenName =request.CanteenName;
    settings.Address = request.Address;
    settings.ContactNumber = request.ContactNumber;
    settings.OpeningTime = TimeSpan.Parse(request.OpeningTime);
    settings.ClosingTime =TimeSpan.Parse(request.ClosingTime);
    settings.IsOpen = request.IsOpen;

      await _db.SaveChangesAsync();

        return Ok(new
        {
          status= 200,
          message= "Updated succesfully",
          data = new CanteenDetailResponse
          {
            CanteenName = settings.CanteenName,
            Address =settings.Address,
            ContactNumber = settings.ContactNumber,
            OpeningTime = settings.OpeningTime,
            ClosingTime = settings.ClosingTime,
            IsOpen =settings.IsOpen  
          }
        });
     }

        [HttpPost("update-order-detail")]
    public async Task<IActionResult> UpdateOrderDetail(CanteenOrderRequest request)
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

    
       var settings = await _db.Settings.FirstOrDefaultAsync();

    if (settings == null)
    {
        return Ok(new
        {
            status = 200,
            message = "No data found",
            data = (object?)null
        });
    }

    settings.IsOnlineOrderingEnabled =request.IsOnlineOrderingEnabled;
    settings.RequirePickupCode = request.RequirePickupCode;
    settings.MaxActiveOrders = request.MaxActiveOrders;
  

      await _db.SaveChangesAsync();

        return Ok(new
        {
          status= 200,
          message= "Updated succesfully",
          data = new CanteenOrderResponse
          {
           IsOnlineOrderingEnabled = settings.IsOnlineOrderingEnabled,
           RequirePickupCode = settings.RequirePickupCode,
           MaxActiveOrders = settings.MaxActiveOrders
          }
        });
     }

}

public class AdminProfileRequest{
  public required string ApiKey { get; set; }
  public required string FullName{get; set;}
  public string? MobileNumber{get; set;}
   public IFormFile? Image { get; set; }

}
public class AdminProfileResponse{
  public required string FullName { get; set; }
  public required string MobileNumber{get; set;}
  public string? ImageUrl { get; set; }

}

public class ChangePasswordRequest{
  public required string ApiKey { get; set; }
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }

}

public class CanteenDetailRequest
{
     public required string ApiKey { get; set; }
    public required string CanteenName { get; set; } 
    public required string Address { get; set; } 
    public required string ContactNumber { get; set; }
    public required string OpeningTime { get; set; }
    public required string ClosingTime { get; set; }
    public bool IsOpen { get; set; }

}
public class CanteenDetailResponse
{
    public required string CanteenName { get; set; } 
    public required string Address { get; set; } 
    public required string ContactNumber { get; set; }
    public required TimeSpan OpeningTime { get; set; }
    public required TimeSpan ClosingTime { get; set; }
    public bool IsOpen { get; set; }

}
public class OrderSettingRequest
{
    public bool IsOnlineOrderingEnabled { get; set; }
    public bool RequirePickupCode { get; set; }
    public int MaxActiveOrders { get; set; }  
}

public class CanteenOrderRequest
{
     public required string ApiKey { get; set; }
    public bool IsOnlineOrderingEnabled { get; set; }
    public bool RequirePickupCode { get; set; }
    public int MaxActiveOrders { get; set; }

}
public class CanteenOrderResponse
{
    public bool IsOnlineOrderingEnabled { get; set; }
    public bool RequirePickupCode { get; set; }
    public int MaxActiveOrders { get; set; }

}
public class DetailsResponse{
    public required string CanteenName { get; set; } 
    public required string Address { get; set; } 
    public required string ContactNumber { get; set; }
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public bool IsOpen { get; set; }
    public bool IsOnlineOrderingEnabled { get; set; }
    public bool RequirePickupCode { get; set; }
    public int MaxActiveOrders { get; set; }
     public required DateTime AccountCreated { get; set; }  
     public  DateTime? LastLogin { get; set; } 

}
