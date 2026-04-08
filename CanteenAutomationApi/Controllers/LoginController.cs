using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CanteenBackend.Data;
using CanteenBackend.Models;

[ApiController]
[Route("api/login")]
public class LoginController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public LoginController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

[HttpPost("admin")] 
public IActionResult Login(LoginRequest request)
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

    //  Get user from DB
    var dbUser = _db.Users
        .Include(u => u.Role)
        .FirstOrDefault(u => u.Email == request.Email);

    //  Validate credentials
     if (dbUser == null ||
         !PasswordHelper.Verify(request.Password, dbUser.PasswordHash!))
     {
         return Unauthorized(new
         {
             status = 401,
             message = "Invalid email or password",
             data = (object?)null
         });
     }


    //  Generate JWT
    var jwtToken = JwtHelper.GenerateToken(dbUser, _config);

    dbUser.IsActive = true;
    _db.SaveChanges();

    //  Return response
    return Ok(new
    {
        status = 200,
        message = "Login successful",
        data = new
        {
           user = new
         {
            dbUser.FullName,
            dbUser.Email,
            Role = dbUser.Role.Name,        
         },
        token = jwtToken,
        }
    });
}

[HttpPost("send-otp")]
public IActionResult SendOtp(SendOtpRequest request)
{
    if (request.ApiKey != Constants.api)
    {
        return Unauthorized(new
        {
            status = 401,
            message = "Invalid API key"
        });
    }

    var user = _db.Users.FirstOrDefault(u => u.MobileNumber == request.MobileNumber);

    if (user == null)
    {
        user = new User
        {
            MobileNumber = request.MobileNumber,
            FullName = "New User",
            RoleId = 3, // user role
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
    }

    var otp = new Random().Next(1000, 9999).ToString();

    user.OtpCode = otp;
    user.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

    _db.SaveChanges();

    // TODO: Send SMS here

    return Ok(new
    {
        status = 200,
        message = "OTP sent successfully",
        data = new
        {
            code = otp
        }
    });
}

[HttpPost("verify-otp")]
public IActionResult VerifyOtp(VerifyOtpRequest request)
{
    if (request.ApiKey != Constants.api)
    {
        return Unauthorized(new
        {
            status = 401,
            message = "Invalid API key"
        });
    }

    var user = _db.Users
        .Include(u => u.Role)
        .FirstOrDefault(u => u.MobileNumber == request.MobileNumber);

    if (user == null ||
        user.OtpCode != request.Otp ||
        user.OtpExpiry < DateTime.UtcNow)
    {
        return Unauthorized(new
        {
            status = 401,
            message = "Invalid or expired OTP"
        });
    }

    user.OtpCode = null;
    user.OtpExpiry = null;

    var token = JwtHelper.GenerateToken(user, _config);

    _db.SaveChanges();

    return Ok(new
    {
        status = 200,
        message = "Login successful",
        data = new
        {
            user = new
            {
                user.FullName,
                user.MobileNumber,
                Role = user.Role.Name
            },
            token = token
        }
    });
}
}
    public class LoginRequest
{
     public required string ApiKey { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class SendOtpRequest
{
    public required string ApiKey { get; set; }
    public required string MobileNumber { get; set; }
}

public class VerifyOtpRequest
{
    public required string ApiKey { get; set; }
    public required string MobileNumber { get; set; }
    public required string Otp { get; set; }
}