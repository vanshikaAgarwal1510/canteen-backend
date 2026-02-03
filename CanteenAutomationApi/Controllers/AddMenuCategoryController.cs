using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[Authorize]
[ApiController]
[Route("api/add-menu-categories")]
public class AddMenuCategoryController : ControllerBase
{
 private readonly AppDbContext _db;
   public AddMenuCategoryController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
     public async Task<IActionResult> AddMenuCategory([FromBody] AddMenuCategoryRequest request)
    {
       if(_db.MenuCategories.Any(c => c.Name == request.Name))
       {
        return BadRequest(new
        {
            status = 400,
            message = "Menu category already exists",
            data = (object?)null
        });
       }
       var newCategory = new MenuCategory
       {
           Name = request.Name
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
}

public class AddMenuCategoryRequest
{
    public required string Name { get; set; }

}

