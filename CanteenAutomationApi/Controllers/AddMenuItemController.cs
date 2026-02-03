using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[Authorize]
[ApiController]
[Route("api/add-menu-items")]
public class AddMenuItemController : ControllerBase
{
 private readonly AppDbContext _db;
   public AddMenuItemController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
     public async Task<IActionResult> AddMenuItem([FromBody] AddMenuItemRequest request)
    {
       if(_db.MenuItems.Any(c => c.Name == request.Name))
       {
        return BadRequest(new
        {
            status = 400,
            message = "Menu item already exists",
            data = (object?)null
        });
       }
       var category = await _db.MenuCategories.FindAsync(request.CategoryName);
       if (category == null)
       {
           return BadRequest(new
           {
               status = 400,
               message = "Category not found.Please provide a valid Category or create a new one.",
               data = (object?)null
           });
       }

       var newItem = new MenuItem
       {
           Name = request.Name, 
           Price = request.Price,
           CategoryId = category.Id,
           Category = category,
           IsAvailable = request.IsAvailable
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
                CategoryName = category.Name
           }
       });
    }
}

public class AddMenuItemRequest
{
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required string CategoryName { get; set; }
    public required bool IsAvailable { get; set; }


}
public class MenuItemResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } =null!;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }

    public string CategoryName { get; set; }=null!;
}


