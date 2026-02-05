using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

[ApiController]
[Route("api/signup")]
public class SignupController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public SignupController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
    
        var emailExists = await _db.Users
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
            return BadRequest(new
            {
                status = 400,
                message = "Email already exists",
                data = (object?)null
            });

    
        var role = await _db.Roles
            .FirstOrDefaultAsync(r => r.Name == "User");

        if (role == null)
            return BadRequest(new
            {
                status = 400,
                message = "User role not found",
                data = (object?)null
            });

    
        var newUser = new User
        {
            FullName = request.Name,
            Email = request.Email,
            PasswordHash = PasswordHelper.Hash(request.Password),
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();

    
        newUser = await _db.Users
            .Include(u => u.Role)
            .FirstAsync(u => u.Id == newUser.Id);
            
        var jwtToken = JwtHelper.GenerateToken(newUser, _config);

        return Ok(new
        {
            status = 200,
            message = "Signup successful",
            data = new
            {
                user = new
                {
                    newUser.FullName,
                    newUser.Email,
                    Role = newUser.Role.Name
                },
                token = jwtToken
            }
        });
    }
}
public class SignupRequest
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}