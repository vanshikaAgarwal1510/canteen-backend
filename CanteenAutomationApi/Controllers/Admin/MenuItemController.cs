using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class MenuItemController : ControllerBase
{
 private readonly AppDbContext _db;
   public MenuItemController(AppDbContext db)
    {
        _db = db;
    }

  
    [HttpPost("add-menu-item")]
     public async Task<IActionResult> AddMenuItem([FromForm] AddMenuItemRequest request)
    {
       if(_db.MenuItems.Any(c => c.Name == request.Name && !c.IsDeleted))
       {
        return BadRequest(new
        {
            status = 400,
            message = "Menu item already exists",
            data = (object?)null
        });
       }
       var category = await _db.MenuCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDeleted);
       if (category == null)
       {
           return BadRequest(new
           {
               status = 400,
               message = "Category not found.Please provide a valid Category or create a new one.",
               data = (object?)null
           });
       }

         string? imageUrl = null;

       if (request.Image != null)
        {
        //  Create unique file name
        var fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);

        //  Decide where to save
        var folderPath = Path.Combine("wwwroot/uploads/items");

        Directory.CreateDirectory(folderPath);

        //  Full file path
        var fullPath = Path.Combine(folderPath, fileName);

        //  Save image to folder
        using var stream = new FileStream(fullPath, FileMode.Create);
        await request.Image.CopyToAsync(stream);

        // Save path in DB
        imageUrl = "/uploads/items/" + fileName;
        }
        
       var newItem = new MenuItem
       {
           Name = request.Name, 
           Price = request.Price,
           CategoryId = category.Id,
           Category = category,
           IsAvailable = request.IsAvailable,
           ImageUrl = imageUrl
       };

       _db.MenuItems.Add(newItem);
       await _db.SaveChangesAsync();
       
       return Ok(new
       {
           status = 200,
           message = "Menu item added successfully",
           data = new MenuItemResponseDto
           {
                Id = newItem.Id,
                Name = newItem.Name,
                Price = newItem.Price,
                IsAvailable = newItem.IsAvailable,
                ImageUrl = newItem.ImageUrl,
                CategoryId = category.Id
           }
       });
    }
    
    [HttpPost("update-menu-item")]
    public async Task<IActionResult> UpdateMenuItem([FromForm] UpdateItemRequest request){
         var item =  await _db.MenuItems.FirstOrDefaultAsync(i => i.Id == request.ItemId);
         if(item == null || item.IsDeleted)
         {
        return NotFound(new
        {
            status = 404,
            message = "Item not found",
            data = (object?)null
        });
            }


        item.Name = request.Name;
        item.Price = request.Price;
        item.IsAvailable = request.IsAvailable;
        item.CategoryId = request.CategoryId;

         // If new image uploaded → replace old one
         if (request.Image != null)
    {
        // delete old image
        if (!string.IsNullOrEmpty(item.ImageUrl))
        {
            var oldPath = Path.Combine(
                "wwwroot",
                item.ImageUrl.TrimStart('/')
            );

            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        // save new image
        var fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);
        var folderPath = Path.Combine("wwwroot/uploads/items");

        Directory.CreateDirectory(folderPath);

        var fullPath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await request.Image.CopyToAsync(stream);

        item.ImageUrl = "/uploads/items/" + fileName;
    }

            await _db.SaveChangesAsync();
            
        return Ok(new
        {
            status = 200,
            message = "Updated successfully",
            data =  new UpdateItemResponse{  
                Name = item.Name,
                Price = item.Price,
                CategoryId = item.CategoryId,
                ImageUrl = item.ImageUrl,
                ItemId = item.Id,
                IsAvailable = item.IsAvailable
        }
        });
    }

    [HttpPost("delete-menu-item")]
    public async Task<IActionResult> DeleteMenuItem(int id)
{
  
    var item = await _db.MenuItems.FindAsync(id);

    if (item == null || item.IsDeleted)
         return NotFound(new
        {
            status = 404,
            message = "Item not found",
            data = (object?)null
        });

   
    if (!string.IsNullOrEmpty(item.ImageUrl))
    {
        var imagePath = Path.Combine(
            "wwwroot",
            item.ImageUrl.TrimStart('/')
        );

        if (System.IO.File.Exists(imagePath))
        {
            System.IO.File.Delete(imagePath);
        }
    }

    
    item.IsDeleted = true;
    await _db.SaveChangesAsync();

    return Ok(new
    {   status = 200,
        message = "Menu item deleted successfully"
    });
}


}

public class AddMenuItemRequest
{
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required int CategoryId { get; set; }
    public required bool IsAvailable { get; set; }
    public IFormFile? Image { get; set; }


}
public class MenuItemResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } =null!;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
}

public class UpdateItemRequest
{
    public int ItemId { get; set; }
    public bool IsAvailable { get; set; }
     public required string Name { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public IFormFile? Image { get; set; } // optional
}
public class UpdateItemResponse
{
    public int ItemId { get; set; }
    public bool IsAvailable { get; set; }
     public required string Name { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; } // optional
}


