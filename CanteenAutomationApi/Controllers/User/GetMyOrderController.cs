using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Claims;


[Authorize(Roles ="User")]
[ApiController]
[Route("api/my-order")]
public class GetMyOrderController : ControllerBase
{
     private readonly AppDbContext _db;

    public GetMyOrderController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> GetOrders()
    {
         var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
         if(string.IsNullOrEmpty(userIdClaim)) 
             return Unauthorized(new
             {
                 status = 401,
                 message = "Invalid or missing user ID",
                 data = (object?)null
             });
         int userId = int.Parse(userIdClaim);

        var orders = await _db.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Item)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderResponseDto
            {
                OrderId = o.Id,
                OrderDate = o.CreatedAt,
                Status = o.Status,
                TotalAmount = o.Total,
                PaymentStatus = o.Payment != null ? o.Payment.PaymentStatus : "Pending",
                OrderType = o.OrderType,

                UserId = o.User.Id,
                UserName = o.User.FullName,

                Items = o.Items.Select(oi => new OrderItemDto
                {
                    ItemId = oi.ItemId,
                    ItemName = oi.Item!.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            })
            .ToListAsync();

        return Ok(new
        {
            status = 200,
            message = "Orders fetched successfully",
            data = orders
        });
    }
}
