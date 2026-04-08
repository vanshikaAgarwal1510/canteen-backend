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
    public async Task<IActionResult> GetStaff(GetStaffRequest request)
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
        var staff = await _db.Users
            .Where(u => u.Role.Name == "Staff")
            .Select(u => new AddStaffResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email!,
                Role = u.Role.Name,
                StaffType = u.StaffType ?? 0,
                IsActive = u.IsActive,
                ImageUrl = u.ImageUrl

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
    public async Task<IActionResult> AddStaff([FromForm]AddStaffRequest request)
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

       string? imageUrl = null;

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
        //  Create unique file name
        var fileName = Guid.NewGuid() + extension;

        //  Decide where to save
        var folderPath = Path.Combine("wwwroot/uploads/profile");

        Directory.CreateDirectory(folderPath);

        //  Full file path
        var fullPath = Path.Combine(folderPath, fileName);

        //  Save image to folder
        using var stream = new FileStream(fullPath, FileMode.Create);
        await request.Image.CopyToAsync(stream);

        // Save path in DB
        imageUrl = "/uploads/profile/" + fileName;
        }
        
        var staff = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = PasswordHelper.Hash(request.Password),
            RoleId = staffRole.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            StaffType = request.StaffType,
            ImageUrl = imageUrl
        };



        _db.Users.Add(staff);
        await _db.SaveChangesAsync();

        return Ok(new
       {
          status = 200,
          message = "Staff added successfully",
          data = new AddStaffResponse
              {
                    FullName = staff.FullName,
                    Email = staff.Email,
                    Role = staffRole.Name,
                    ImageUrl = staff.ImageUrl,
                    StaffType = request.StaffType,
                    IsActive = staff.IsActive,
                   
              }
        
      });
    }
    [HttpPost("update-staff")]
    public async Task<IActionResult> UpdateStaff(UpdateStaffRequest request)
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

    var emailExists = await _db.Users
    .AnyAsync(u => u.Email == request.Email && u.Id != request.Id);

    if (emailExists)
    {
        return BadRequest(new
    {
        status = 400,
        message = "Email already in use",
        data = (object?)null
    });
    }

    staff.FullName = request.FullName;
    staff.StaffType = request.StaffType;
    staff.Email = request.Email;
    staff.IsActive = request.IsActive;


    // if (!string.IsNullOrWhiteSpace(request.Password))
    // {
    // staff.PasswordHash = PasswordHelper.Hash(request.Password);
    // }

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
        if (!string.IsNullOrEmpty(staff.ImageUrl))
        {
            var oldPath = Path.Combine(
                "wwwroot",
                staff.ImageUrl.TrimStart('/')
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

        staff.ImageUrl = "/uploads/profile/" + fileName;
    }

           

    await _db.SaveChangesAsync();

    return Ok(new
    {
        status = 200,
        message = "Staff updated successfully",
        data =new AddStaffResponse
              {
                    FullName = staff.FullName,
                    Email = staff.Email,
                    Role = staff.Role.Name,
                     ImageUrl = staff.ImageUrl,
                    StaffType = staff.StaffType??1,
                    IsActive = staff.IsActive,
                
              }
    });
} 

    [HttpPost("delete-staff")]
    public async Task<IActionResult> DeleteStaff([FromBody] DeleteStaffRequest request)
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
         var staff = await _db.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Id == request.Id);
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
public class GetStaffRequest
{
    public required string ApiKey { get; set; }
}
public class AddStaffRequest
{
    public required string ApiKey { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public int StaffType { get; set; }
    public IFormFile? Image { get; set; }
}

public class AddStaffResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public int StaffType { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateStaffRequest
{
    public required string ApiKey { get; set; }
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
     public bool IsActive { get; set; }
    public int? StaffType { get; set; }
    public IFormFile? Image { get; set; }

}

public class DeleteStaffRequest
{
    public required string ApiKey { get; set; }
    public int Id { get; set; }
}