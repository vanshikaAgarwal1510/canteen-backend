using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/payment-method")]
public class PaymentMethodController : ControllerBase
{
    private readonly AppDbContext _db;

    public PaymentMethodController(AppDbContext db)
    {
        _db = db;
    }
    [HttpPost("get-payment-method")]
    public async Task<IActionResult> GetPaymentMethodList(GetPaymentMethodRequest request)
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
        var method = await _db.PaymentMethods.OrderBy(u => u.Id)
            .Select(u => new AddPaymentMethodResponse
            {
                Id = u.Id,
                Name = u.Name,
                IsEnabled = u.IsEnabled,
                Description = u.Description,
                Deletable = u.IsMethodDeletable,
                ImageUrl = u.ImageUrl
                


            })
            .ToListAsync();

        if (method.Count == 0)
            return NotFound(new
            {
                status = 404,
                message = "Payment method not found",
                data = (object?)null
            });

        return Ok(new
        {
            status = 200,
            message = "Payment method retrieved successfully",
            data = method
        });
    }

    [HttpPost("add-payment-method")]
    public async Task<IActionResult> AddPaymentMethod(AddPaymentMethodRequest request)
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
        var existingMethod = await _db.PaymentMethods
            .AnyAsync(u => u.Name.ToLower() == request.Name.ToLower());

        if (existingMethod)
            return BadRequest(new
            {
                status = 400,
                message = "Payment Method already exists",
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
        var folderPath = Path.Combine("wwwroot/uploads/payment");

        Directory.CreateDirectory(folderPath);

        //  Full file path
        var fullPath = Path.Combine(folderPath, fileName);

        //  Save image to folder
        using var stream = new FileStream(fullPath, FileMode.Create);
        await request.Image.CopyToAsync(stream);

        // Save path in DB
        imageUrl = "/uploads/payment/" + fileName;
        }




        var method = new PaymentMethod
        {
            Name = request.Name,
            IsEnabled = true,
            Description = request.Description,
            IsMethodDeletable = false,
            ImageUrl = imageUrl
        };



        _db.PaymentMethods.Add(method);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            status = 200,
            message = "Payment Method added successfully",
            data = new AddPaymentMethodResponse
            {
                Id = method.Id,
                Name = method.Name,
                IsEnabled = method.IsEnabled,
                Description = method.Description,
                ImageUrl = method.ImageUrl
            }

        });
    }
    [HttpPost("update-payment-method")]
    public async Task<IActionResult> UpdatePaymentMethod(UpdatePaymentMethodRequest request)
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

        var method = await _db.PaymentMethods
            .FirstOrDefaultAsync(u => u.Id == request.Id);


        if (method == null)
            return BadRequest(new
            {
                status = 400,
                message = "Payment Method does not exist",
                data = (object?)null
            });

        if (!request.IsEnabled && !method.IsMethodDeletable)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Can't update for this payment Method",
                data = (object?)null
            });
        }
         // If new image uploaded → replace old one
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
        if (!string.IsNullOrEmpty(method.ImageUrl))
        {
            var oldPath = Path.Combine(
                "wwwroot",
                method.ImageUrl.TrimStart('/')
            );

            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }
        

        // save new image
        var fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);
        var folderPath = Path.Combine("wwwroot/uploads/payment");

        Directory.CreateDirectory(folderPath);

        var fullPath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await request.Image.CopyToAsync(stream);

        method.ImageUrl = "/uploads/payment/" + fileName;
    }


        method.IsEnabled = request.IsEnabled;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            status = 200,
            message = "Payment Method updated successfully",
            data = new AddPaymentMethodResponse
            {
                Id = method.Id,
                Name = method.Name,
                IsEnabled = method.IsEnabled,
                Description = method.Description,
                ImageUrl = method.ImageUrl


            }
        });
    }

    [HttpPost("delete-payment-method")]
    public async Task<IActionResult> DeletePaymentMethod([FromBody] DeletePaymentMethodRequest request)
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
        var method = await _db.PaymentMethods
       .FirstOrDefaultAsync(u => u.Id == request.Id);

        if (method == null)
            return NotFound(new
            {
                status = 404,
                message = "Coupon not found",
                data = (object?)null
            });

        if (!method.IsMethodDeletable)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Can't delete  this Payment Method",
                data = (object?)null
            });
        }



        _db.PaymentMethods.Remove(method);
        await _db.SaveChangesAsync();


        return Ok(new
        {
            status = 200,
            message = "Payment method deleted successfully",
            data = (object?)null
        });
    }
}

public class GetPaymentMethodRequest
{
    public required string ApiKey { get; set; }
}
public class AddPaymentMethodRequest
{
    public required string ApiKey { get; set; }
    public required string Name { get; set; }

    public IFormFile? Image { get; set; }

    public required string Description { get; set; }



}

public class AddPaymentMethodResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsEnabled { get; set; }

    public bool Deletable { get; set; }
    public string? ImageUrl { get; set; }

    public required string Description { get; set; }

}

public class UpdatePaymentMethodRequest
{
    public required string ApiKey { get; set; }
    public int Id { get; set; }
    public bool IsEnabled { get; set; }
    public IFormFile? Image { get; set; }
 
}

public class DeletePaymentMethodRequest
{
    public required string ApiKey { get; set; }
    public int Id { get; set; }
}



