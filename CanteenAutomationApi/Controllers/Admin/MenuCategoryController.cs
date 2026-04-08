using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class MenuCategoryController : ControllerBase
{
 private readonly AppDbContext _db;
   public MenuCategoryController(AppDbContext db)
    {
        _db = db;
    }

[Authorize(Roles = "Admin")]
[HttpPost("add-menu-category")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> AddMenuCategory(  [FromForm] AddMenuCategoryRequest request)
   
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

    if (await _db.MenuCategories.AnyAsync(c =>
        c.Name.ToLower() == request.Name.ToLower() && !c.IsDeleted))
    {
        return BadRequest(new
        {
            status = 400,
            message = "Menu category already exists",
            data = (object?)null
        });
    }

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

        var fileName = Guid.NewGuid() + extension;
        var folderPath = Path.Combine("wwwroot/uploads/categories");

        Directory.CreateDirectory(folderPath);

        var fullPath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await request.Image.CopyToAsync(stream);

        imageUrl = "/uploads/categories/" + fileName;
    }

    var newCategory = new MenuCategory
    {
        Name = request.Name,
        ImageUrl = imageUrl,
        CategoryDescription = request.CategoryDescription
    };

    _db.MenuCategories.Add(newCategory);
    await _db.SaveChangesAsync();

    return Ok(new
    {
        status = 200,
        message = "Menu category added successfully",
        data = new
        {
           categoryId = newCategory.Id,
           categoryName = newCategory.Name,
           imageUrl= newCategory.ImageUrl,
           categoryDescription =  newCategory.CategoryDescription
        }
    });
} 

 [Authorize(Roles = "Admin")]
    [HttpPost("update-menu-category")]   
    public async Task<IActionResult> UpdateCategory([FromForm] UpdateCategoryRequest request){
       
        if (request.ApiKey != Constants.api)
    {
        return Unauthorized(new
        {
            status = 401,
            message = "Invalid API key",
            data = (object?)null
        });
    }
         var category = await _db.MenuCategories.FindAsync(request.CategoryId);
         if(category == null || category.IsDeleted)
         {
        return NotFound(new
        {
            status = 404,
            message = "Category not found",
            data = (object?)null
        });
            }
    
        category.Name = request.Name;
        category.CategoryDescription = request.CategoryDescription;

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

            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                var oldPath = Path.Combine(
                    "wwwroot",
                    category.ImageUrl.TrimStart('/')
                );

                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

        
            var fileName = Guid.NewGuid() + extension;
            var folderPath = Path.Combine("wwwroot/uploads/categories");

            Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await request.Image.CopyToAsync(stream);

            category.ImageUrl = "/uploads/categories/" + fileName;
        }

         await _db.SaveChangesAsync();
            
        return Ok(new
        {
            status = 200,
            message = "Category updated successfully",
            data =  new UpdateCategoryResponse{  
                CategoryName = category.Name,
                ImageUrl = category.ImageUrl,
                CategoryDescription = category.CategoryDescription,
                CategoryId = category.Id
        }
        });
    }

   [Authorize(Roles = "Admin")]
   [HttpPost("delete-menu-category")]
    public async Task<IActionResult> DeleteCategory(DeleteCategoryRequest request)
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

    var category = await _db.MenuCategories.FindAsync(request.CategoryId);

    if (category == null || category.IsDeleted)
         return NotFound(new
        {
            status = 404,
            message = "Category not found",
            data = (object?)null
        });

   
    if (!string.IsNullOrEmpty(category.ImageUrl))
    {
        var imagePath = Path.Combine(
            "wwwroot",
            category.ImageUrl.TrimStart('/')
        );

        if (System.IO.File.Exists(imagePath))
        {
            System.IO.File.Delete(imagePath);
        }
    }

    
    category.IsDeleted = true;

    
    var items = _db.MenuItems.Where(i => i.CategoryId == request.CategoryId );
    foreach (var item in items)
    {
        item.IsDeleted = true;
    }

   
    await _db.SaveChangesAsync();

    return Ok(new
    {   status = 200,
        message = "Menu category deleted successfully"
    });
}


public class AddMenuCategoryRequest
{
    public required string ApiKey { get; set; }
    public required string Name { get; set; }

    public required string CategoryDescription { get; set; }

   public IFormFile? Image { get; set; }

}


public class UpdateCategoryRequest
{
    public required string ApiKey { get; set; }
    public int CategoryId { get; set; }
     public required string Name { get; set; }
     public required string CategoryDescription { get; set; }
    public IFormFile? Image { get; set; } // optional
}
public class UpdateCategoryResponse
{
    
    public int CategoryId { get; set; }
     public required string CategoryName { get; set; }
     public required string CategoryDescription { get; set; }
    public string? ImageUrl { get; set; } // optional
}
public class DeleteCategoryRequest
{
    public required string ApiKey { get; set; }
    public int CategoryId { get; set; }
}
}

