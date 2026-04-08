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
         if (request.ApiKey != Constants.api)
    {
        return Unauthorized(new
        {
            status = 401,
            message = "Invalid API key",
            data = (object?)null
        });
    }
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
           ItemDescription = request.ItemDescription,
           Category = category,
           IsAvailable = request.IsAvailable,
           ImageUrl = imageUrl
       };

       _db.MenuItems.Add(newItem);
       await _db.SaveChangesAsync();

        //  Fetch ratings
        var ratingList = await _db.Ratings
            .GroupBy(r => r.MenuItemId)
            .Select(g => new
            {
                MenuItemId = g.Key,
                AvgRating = g.Average(r => (double)r.Stars),
                RatingCount = g.Count()
            })
            .ToListAsync();

        var ratingLookup = ratingList
            .ToDictionary(x => x.MenuItemId);
       
       return Ok(new
       {
           status = 200,
           message = "Menu item added successfully",
           data = new MenuItemResponseDto
           {
                ItemId = newItem.Id,
                ItemName = newItem.Name,
                Price = newItem.Price,
                IsAvailable = newItem.IsAvailable,
                ImageUrl = newItem.ImageUrl,
                CategoryId = category.Id,
                ItemDescription = newItem.ItemDescription,
                AverageRating =  ratingLookup.ContainsKey(newItem.Id) ? Math.Round(ratingLookup[newItem.Id].AvgRating, 1): 0
                 
           }
       });
    }
    
    [HttpPost("update-menu-item")]
    public async Task<IActionResult> UpdateMenuItem([FromForm] UpdateItemRequest request){
        
         if (request.ApiKey != Constants.api)
    {
        return Unauthorized(new
        {
            status = 401,
            message = "Invalid API key",
            data = (object?)null
        });
    }
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
        item.ItemDescription = request.ItemDescription;

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

           //  Fetch ratings
        var ratingList = await _db.Ratings
            .GroupBy(r => r.MenuItemId)
            .Select(g => new
            {
                MenuItemId = g.Key,
                AvgRating = g.Average(r => (double)r.Stars),
                RatingCount = g.Count()
            })
            .ToListAsync();

        var ratingLookup = ratingList
            .ToDictionary(x => x.MenuItemId);  

        return Ok(new
        {
            status = 200,
            message = "Updated successfully",
            data =  new UpdateItemResponse{  
                ItemName = item.Name,
                Price = item.Price,
                CategoryId = item.CategoryId,
                ImageUrl = item.ImageUrl,
                ItemId = item.Id,
                IsAvailable = item.IsAvailable,
                ItemDescription = item.ItemDescription,
                AverageRating =  ratingLookup.ContainsKey(item.Id) ? Math.Round(ratingLookup[item.Id].AvgRating, 1): 0
             
        }
        });
    }

    [HttpPost("delete-menu-item")]
    public async Task<IActionResult> DeleteMenuItem(DeleteItemRequest request)
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

    var item = await _db.MenuItems.FindAsync(request.Id);

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
    public required string ApiKey { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
   public required string ItemDescription { get; set; }
    public required int CategoryId { get; set; }
    public required bool IsAvailable { get; set; }
    public IFormFile? Image { get; set; }


}
public class MenuItemResponseDto
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = null!;

    public string ItemDescription {get; set;}=null!;

    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public double AverageRating { get; set; }
     public int CategoryId { get; set; } 


}

public class UpdateItemRequest
{
    public required string ApiKey { get; set; }
    public int ItemId { get; set; }
    public bool IsAvailable { get; set; }
     public required string Name { get; set; }
    public required string ItemDescription { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public IFormFile? Image { get; set; } // optional
}
public class UpdateItemResponse
{
    public int ItemId { get; set; }
    public bool IsAvailable { get; set; }
     public required string ItemName { get; set; }
    public required string ItemDescription { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; } // optional
    public double AverageRating { get; set; }
}
public class DeleteItemRequest
{
    public required string ApiKey { get; set; }
    public int Id { get; set; }
}

