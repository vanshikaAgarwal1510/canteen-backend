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
    public async Task<IActionResult> GetMenu()
    {

        var menu = await _db.MenuCategories
            .Where(c => !c.IsDeleted)
            .Include(c => c.Items)
            .Select(c => new MenuCategoryDto
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                ImageUrl = c.ImageUrl,
                Items = c.Items.Where(i => !i.IsDeleted).Select(i => new MenuItemDto
                {
                    ItemId = i.Id,
                    ItemName = i.Name,
                    Price = i.Price,
                    IsAvailable = i.IsAvailable,
                    ImageUrl = i.ImageUrl,
                    AverageRating = _db.Ratings.Where(r => r.ItemId == i.Id).Average(r => (decimal?)r.Stars) ?? 0,
                      }).ToList()
            }) .ToListAsync();

        return Ok(new
        {
            status = 200,
            message = "Menu fetched successfully",
            data = menu
        });
    }
}
class MenuCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public List<MenuItemDto> Items { get; set; } = new List<MenuItemDto>();
}

class MenuItemDto
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = null!;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public decimal AverageRating { get; set; }



}

