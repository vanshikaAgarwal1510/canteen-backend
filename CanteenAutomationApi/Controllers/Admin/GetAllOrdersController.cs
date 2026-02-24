using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CanteenBackend.Models;



[ApiController]
[Route("api/get-orders")]
public class GetOrdersController : ControllerBase
{
     private readonly AppDbContext _db;

    public GetOrdersController(AppDbContext db)
    {
        _db = db;
    }
  
   [Authorize(Roles ="Admin")]
    [HttpPost("all")]
    public async Task<IActionResult> GetAllOrders([FromBody] FilteredOrdersRequest request)
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

        var ordersQuery = _db.Orders.AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
        {
            ordersQuery = ordersQuery.Where(o => o.Status == request.Status);
        }

        if (request.OrderType.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderType == request.OrderType.Value);
        }

        if (!string.IsNullOrEmpty(request.PaymentStatus))
        {
            ordersQuery = ordersQuery.Where(o => o.Payment != null && o.Payment.PaymentStatus == request.PaymentStatus);
        }

        if (!string.IsNullOrEmpty(request.FromDate))
        {
            if (DateTime.TryParse(request.FromDate, out DateTime fromDate))
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= fromDate);
            }
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
                TotalAmount = o.FinalAmount,
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

    [Authorize(Roles ="Staff")]
    [HttpPost("active")]
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
        TotalAmount = o.FinalAmount,
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

    [Authorize(Roles ="Admin")]
        [HttpPost("details")]
    public async Task<IActionResult> GetOrderDetails([FromBody] OrderDetailsRequestDto request)
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

        var order = await _db.Orders
            .Where(o => o.Id == request.OrderId)
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Item)
            .Select(o => new OrderResponseDetailsDto
            {
                OrderId = o.Id,
                OrderDate = o.CreatedAt,
                OrderType=o.OrderType,
                Status = o.Status,
                UserName = o.User.FullName,
                UserNumber = "123456789", // Assuming you have a phone number field in User model, replace with actual field

            
                PaymentStatus = o.Payment != null ? o.Payment.PaymentStatus : "Pending",
                PaymentMode = o.Payment != null ? o.Payment.PaymentMode : 0,
                paidAt = o.Payment != null ? o.Payment.PaidAt : null,

                TotalAmount = o.FinalAmount,
                SubTotal = o.Items.Sum(oi => (double)oi.Price * oi.Quantity),
                Discount = o.Discount,
                Items = o.Items.Select(oi => new OrderItemImageDto
                {
                    ItemId = oi.ItemId,
                    ItemName = oi.Item!.Name,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    TotalPrice = oi.Price * oi.Quantity,
                    ImageUrl = "http://localhost:5123" + (oi.Item.ImageUrl ?? string.Empty)
                }).ToList()


    })
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return NotFound(new
            {
                status = 404,
                message = "Order not found",
                data = (object?)null
            }); 
        }

        return Ok(new
        {
            status = 200,
            message = "Order details fetched successfully",
            data = order
        });
}}
public class FilteredOrdersRequest
{
    public required string ApiKey { get; set; }
     public string Status { get; set; } = string.Empty;
    public string? FromDate { get; set; } = null;
     public int? OrderType{get; set;} =null;
     public string? PaymentStatus { get; set; } = null!;

}

public class OrderDetailsRequestDto
{
    public required string ApiKey { get; set; }
    public int OrderId { get; set; }
}

public class OrderResponseDetailsDto
{
    // Basic order info
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; } 
     public int OrderType { get; set; }
    public string Status { get; set; } = null!;

  
  // User info
    public string UserName { get; set; } = null!;
    public string UserNumber { get; set; } = null!;

    // Payment info

    public int PaymentMode { get; set; }
    public string PaymentStatus { get; set; } = null!;
    public DateTime? paidAt { get; set; }

     //Billing info
    public decimal TotalAmount { get; set; }  
    public double SubTotal { get; set; }
    public decimal Discount { get; set; }

    // Items
    public List<OrderItemImageDto> Items { get; set; } = new List<OrderItemImageDto>();


}

 public class  OrderItemImageDto{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string ImageUrl { get; set; } = null!;
}


