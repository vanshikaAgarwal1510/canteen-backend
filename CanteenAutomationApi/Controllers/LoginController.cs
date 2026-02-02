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
    // 1️⃣ Get user from DB
    var dbUser = _db.Users
        .Include(u => u.Role)
        .FirstOrDefault(u => u.Email == request.Email);

    // 2️⃣ Validate credentials
    if (dbUser == null ||
        !PasswordHelper.Verify(request.Password, dbUser.PasswordHash))
    {
        return Unauthorized("Invalid email or password");
    }

    // 3️⃣ Generate JWT
    var jwtToken = JwtHelper.GenerateToken(dbUser, _config);

    // 4️⃣ Return response
    return Ok(new
    {
        token = jwtToken,
        user = new
        {
            dbUser.Id,
            dbUser.FullName,
            dbUser.Email,
            Role = dbUser.Role.Name
        }
    });
}}

    public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}