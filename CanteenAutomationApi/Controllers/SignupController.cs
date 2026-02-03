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
        if (_db.Users.Any(u => u.Email == request.Email))return BadRequest(new
        {
            status = 400,
            message = "Email already exists",
            data = (object?)null
        });

         var role = await  _db.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
        if (role == null)return BadRequest(new
        {
            status = 400,
            message = "Invalid role",
            data = (object?)null

        });
    

        var newUser = new User
        {
            FullName = request.Name,
            Email = request.Email,
            PasswordHash = PasswordHelper.Hash(request.Password),
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow,
            Role = role
        };

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();

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
    public  required string Role { get; set; }

}