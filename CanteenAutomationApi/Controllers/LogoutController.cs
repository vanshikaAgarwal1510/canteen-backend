using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CanteenBackend.Data;

[ApiController]
[Route("api/logout")]
public class LogoutController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public LogoutController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost]
public IActionResult Logout(LogoutRequest request)
{
  
    var dbUser = _db.Users
        .Include(u => u.Role)
        .FirstOrDefault(u => u.Id == request.UserId);

   

    if (dbUser == null)
        {
        return Unauthorized(new
        {
            status = 401,
            message = "Invalid user ID",
            data = (object?)null
        });
        }

    dbUser.IsActive = false;
    _db.SaveChanges();



    return Ok(new
    {
        status = 200,
        message = "Logout successful",
        data = (object?)null
    });
}}

    public class LogoutRequest
{
    public required int  UserId { get; set; }
}