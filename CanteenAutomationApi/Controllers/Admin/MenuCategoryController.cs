using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


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
     public async Task<IActionResult> AddMenuCategory([FromForm] AddMenuCategoryRequest request)
    {
       if(_db.MenuCategories.Any(c => c.Name == request.Name && !c.IsDeleted))
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
        //  Create unique file name
        var fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);

        //  Decide where to save
        var folderPath = Path.Combine("wwwroot/uploads/categories");

        Directory.CreateDirectory(folderPath);

        //  Full file path
        var fullPath = Path.Combine(folderPath, fileName);

        //  Save image to folder
        using var stream = new FileStream(fullPath, FileMode.Create);
        await request.Image.CopyToAsync(stream);

        // Save path in DB
        imageUrl = "/uploads/categories/" + fileName;
        }
       var newCategory = new MenuCategory
       {
           Name = request.Name,
           ImageUrl = imageUrl
       };

       _db.MenuCategories.Add(newCategory);
       await _db.SaveChangesAsync();
       
       return Ok(new
       {
           status = 200,
           message = "Menu category added successfully",
           data = newCategory
       });
    }
    [Authorize(Roles = "Admin")]
    [HttpPost("update-menu-category")]   
    public async Task<IActionResult> UpdateCategory([FromForm] UpdateCategoryRequest request){
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

        if (request.Image != null)
        {
         
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                var oldPath = Path.Combine(
                    "wwwroot",
                    category.ImageUrl.TrimStart('/')
                );

                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

        
            var fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);
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
                Name = category.Name,
                ImageUrl = category.ImageUrl,
                CategoryId = category.Id
        }
        });
    }

   [Authorize(Roles = "Admin")]
   [HttpPost("delete-menu-category")]
    public async Task<IActionResult> DeleteCategory(int id)
{
  
    var category = await _db.MenuCategories.FindAsync(id);

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

    
    var items = _db.MenuItems.Where(i => i.CategoryId == id);
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


}

public class AddMenuCategoryRequest
{
    public required string Name { get; set; }
   public IFormFile? Image { get; set; }

}


public class UpdateCategoryRequest
{
    public int CategoryId { get; set; }
     public required string Name { get; set; }
    public IFormFile? Image { get; set; } // optional
}
public class UpdateCategoryResponse
{
    public int CategoryId { get; set; }
     public required string Name { get; set; }
    public string? ImageUrl { get; set; } // optional
}

