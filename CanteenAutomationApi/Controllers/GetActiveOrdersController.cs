using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


[Authorize(Roles ="Staff")]
[ApiController]
[Route("api/get-active-orders")]
public class GetActiveOrdersController : ControllerBase
{
     private readonly AppDbContext _db;

    public GetActiveOrdersController(AppDbContext db)
    {
        _db = db;
    }
    [HttpPost]
    public async Task<IActionResult> GetActiveOrders()
    {
       var orders = await _db.Orders
    .Where(o => o.Status != "Completed" && o.Status != "Cancelled")
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
