using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


[Authorize(Roles ="Admin")]
[ApiController]
[Route("api/get-orders")]
public class GetAllOrdersController : ControllerBase
{
     private readonly AppDbContext _db;

    public GetAllOrdersController(AppDbContext db)
    {
        _db = db;
    }
    [HttpPost]
    public async Task<IActionResult> GetOrders([FromBody] FilteredOrdersRequest request)
    {

        var ordersQuery = _db.Orders.AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
        {
            ordersQuery = ordersQuery.Where(o => o.Status == request.Status);
        }

        if (request.FromDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.CreatedAt <= request.ToDate.Value);
        }

        var orders = await ordersQuery
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
public class FilteredOrdersRequest
{
    public string Status { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; } = null;
    public DateTime? ToDate { get; set; }= null;

}
