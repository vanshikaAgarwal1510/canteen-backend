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


         var recommendItems = await _db.OrderItems
           .Where(oi => oi.Order!.Status == 4 && oi.Item!.Category != null && !oi.Item.Category.IsDeleted)
           .GroupBy(oi => new
           {
               oi.ItemId,
               ItemName = oi.Item!.Name,
               ItemDes = oi.Item.ItemDescription,
               Price = oi.Item.Price,
               IsAvailable = oi.Item.IsAvailable,
               ImageUrl = oi.Item.ImageUrl,
               CategoryID = oi.Item.CategoryId
           })
           .Select(g => new MenuItemDto
           {
               ItemId = g.Key.ItemId,
               ItemName = g.Key.ItemName,
               ItemDescription = g.Key.ItemDes,
               Price = g.Key.Price,
               IsAvailable = g.Key.IsAvailable,
               ImageUrl = g.Key.ImageUrl,
               CategoryId = g.Key.CategoryID,


               TotalQuantity = g.Sum(x => x.Quantity)
           })
           .OrderByDescending(x => x.TotalQuantity)
           .Take(10)
           .ToListAsync();



     object? userData = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
            if (int.TryParse(userIdClaim, out int userId))
            {
                userData = await _db.Users
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
            }
        }

        return Ok(new
        {
            status = 200,
            message = "Init Data fetched successfully",
            data = new
            {
                   menu = menu,
                 recommendMenu = recommendItems,
                user = userData
            }
        });
    }

[HttpGet("check-token")]
[Authorize]
public IActionResult CheckToken()
{
    return Ok(new
    {
        isValid = true,
        userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        isAuthenticated = User.Identity?.IsAuthenticated
    });
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
    public int TotalQuantity { get; set; }

    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public double AverageRating { get; set; }
    public int CategoryId { get; set; } 
}



public class InitDataRequest
{
    public required string ApiKey { get; set; }
}
}