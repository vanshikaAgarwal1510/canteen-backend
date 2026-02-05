using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CanteenBackend.Data;

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

    [HttpPost]
public IActionResult Login(LoginRequest request)
{
    //  Get user from DB
    var dbUser = _db.Users
        .Include(u => u.Role)
        .FirstOrDefault(u => u.Email == request.Email);

    //  Validate credentials
     if (dbUser == null ||
         !PasswordHelper.Verify(request.Password, dbUser.PasswordHash))
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
}}

    public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}