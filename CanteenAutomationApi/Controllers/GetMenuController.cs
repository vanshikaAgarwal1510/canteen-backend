using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


[Authorize]
[ApiController]
[Route("api/menu")]
public class MenuController : ControllerBase
{
     private readonly AppDbContext _db;

    public MenuController(AppDbContext db)
    {
        _db = db;
    }
    [HttpPost]
    public async Task<IActionResult> GetMenu([FromBody] GetMenuRequest request)
    {
      if (request.ApiKey != Constants.api)
             {
                 return Unauthorized(new
                 {
                     status = 401,
                     message = "An invalid API key was provided",
                     data = (object?)null
                 });
             }
    
        // 1️⃣ Fetch ratings safely (SQLite-compatible)
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

        // 2️⃣ Fetch menu with categories & items
        var menu = await _db.MenuCategories
            .Where(c => !c.IsDeleted)
            .Include(c => c.Items)
            .Select(c => new MenuCategoryDto
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                CategoryDescription = c.CategoryDescription,
                ImageUrl = c.ImageUrl,

                Items = c.Items
                    .Where(i => !i.IsDeleted)
                    .Select(i => new MenuItemDto
                    {
                        ItemId = i.Id,
                        CategoryId = i.CategoryId,
                        ItemName = i.Name,
                        ItemDescription = i.ItemDescription,
                        Price = i.Price,
                        IsAvailable = i.IsAvailable,
                        ImageUrl = i.ImageUrl,
                        AverageRating = ratingLookup.ContainsKey(i.Id) ? Math.Round(ratingLookup[i.Id].AvgRating, 1): 0
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(new
        {
            status = 200,
            message = "Menu fetched successfully",
            data = menu
        });
    }

class MenuCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string CategoryDescription {get;set;} = null!;

    public string? ImageUrl { get; set; }
    public List<MenuItemDto> Items { get; set; } = new List<MenuItemDto>();
}

class MenuItemDto
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
}
public class GetMenuRequest{
  public required string ApiKey { get; set; }

}


