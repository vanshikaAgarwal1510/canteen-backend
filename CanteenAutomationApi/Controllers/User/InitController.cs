using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/init")]
public class InitController : ControllerBase
{
    private readonly AppDbContext _db;

    public InitController(AppDbContext db)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> InitData(InitDataRequest request)
    {

        if (request.ApiKey != Constants.api)
        {
            return Unauthorized(new
            {
                status = 401,
                message = "Invalid API key"
            });
        }

        // MENU DATA
        //  Fetch ratings safely (SQLite-compatible)
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

        //  Fetch menu with categories & items
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

     
        object? userData = null;


        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userIdClaim, out int userId))
        {
            var user = await _db.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.MobileNumber,
                    u.WalletBalance,
                    u.IsUniversityStudent
                })
                .FirstOrDefaultAsync();

            userData = user;
        }

        return Ok(new
        {
            status = 200,
            message = "Init Data fetched successfully",
            data = new
            {
                menu = menu,
                user = userData
            }
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
}



public class InitDataRequest
{
    public required string ApiKey { get; set; }
}
}