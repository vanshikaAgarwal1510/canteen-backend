using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using CanteenBackend.Data;
using CanteenBackend.Models;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/staff")]
public class StaffController : ControllerBase
{
    private readonly AppDbContext _db;

    public StaffController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("get-staff")]
    public async Task<IActionResult> GetStaff()
    {
        var staff = await _db.Users
            .Where(u => u.Role.Name == "Staff")
            .Select(u => new AddStaffResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.Name,
                StaffType = u.StaffType ?? 0,
                IsActive = u.IsActive
            })
            .ToListAsync();

        if (staff == null || staff.Count == 0)
            return NotFound(new
            {
                status = 404,
                message = "Staff not found",
                data = (object?)null
            });
            
        return Ok(new
       {
          status = 200,
          message = "Staff retrieved successfully",
          data = new
          {
                staff = staff
          }
      });
    }
   
    [HttpPost("add-staff")]
    public async Task<IActionResult> AddStaff(AddStaffRequest request)
    {
    
        var existingUser = await _db.Users
            .AnyAsync(u => u.Email == request.Email);
        if (existingUser)
            return BadRequest(new
        {
            status = 400,
            message = "Email already exists",
            data = (object?)null
        });

        
        var staffRole = await _db.Roles
            .FirstOrDefaultAsync(r => r.Name == "Staff");

        if (staffRole == null)
               return BadRequest(new
        {
            status = 400,
            message = "Staff role not found",
            data = (object?)null
        });

       
        var staff = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = PasswordHelper.Hash(request.Password),
            RoleId = staffRole.Id,
            IsActive = true,
            CreatedAt = DateTime.Now,
            StaffType = request.StaffType
        };


        _db.Users.Add(staff);
        await _db.SaveChangesAsync();

        return Ok(new
       {
          status = 200,
          message = "Staff added successfully",
          data = new
          {
              user = new AddStaffResponse
              {
                    FullName = staff.FullName,
                    Email = staff.Email,
                    Role = staffRole.Name,
                    StaffType = request.StaffType,
                    IsActive = staff.IsActive
              }
          }
      });
    }
    [HttpPost("update-staff")]
    public async Task<IActionResult> UpdateStaff(UpdateStaffRequest request)
{
   
    var staff = await _db.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Id == request.Id);

    if (staff == null)
        return BadRequest(new
        {
            status = 400,
            message = "Staff does not exist",
            data = (object?)null
        });

   
    if (staff.Role.Name != "Staff")
        return BadRequest(new
        {
            status = 400,
            message = "User is not a staff member",
            data = (object?)null
        });

    staff.FullName = request.FullName;
    staff.StaffType = request.StaffType;


    if (!string.IsNullOrWhiteSpace(request.Password))
    {
        staff.PasswordHash = PasswordHelper.Hash(request.Password);
    }

    await _db.SaveChangesAsync();

    return Ok(new
    {
        status = 200,
        message = "Staff updated successfully",
        data = new
        {
            user = new
            {
                staff.Id,
                staff.FullName,
                staff.Email,
                Role = staff.Role.Name,
                StaffType = staff.StaffType ?? 0,
                staff.IsActive
            }
        }
    });
}

    [HttpPost("delete-staff")]
    public async Task<IActionResult> DeleteStaff([FromBody] DeleteStaffRequest request)
    {
         var staff = await _db.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Email == request.Email);
        if (staff == null)
            return NotFound(new
            {
                status = 404,
                message = "Staff not found",
                data = (object?)null
            });

    
        if (staff.Role.Name != "Staff")
            return BadRequest(new
            {
                status = 400,
                message = "User is not a staff member",
                data = (object?)null
            });

    
        staff.IsActive = false;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            status = 200,
            message = "Staff deleted (disabled) successfully",
            data = (object?)null
        });
    }
   
}
public class AddStaffRequest
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }

    public int StaffType { get; set; }
}

public class AddStaffResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;

    public int StaffType { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateStaffRequest
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public int? StaffType { get; set; }
}

public class DeleteStaffRequest
{
    public required string Email { get; set; }
}