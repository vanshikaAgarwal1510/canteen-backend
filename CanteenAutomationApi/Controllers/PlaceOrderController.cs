using System.Security.Claims;
using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;



[Route("api/place-order")]
public class PlaceOrderController : ControllerBase
{
 private readonly AppDbContext _db;
   public PlaceOrderController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "User")]
    [HttpPost]
     public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        if(request.Items == null || !request.Items.Any())
        {
            return BadRequest(new
            {
                status = 400,
                message = "Order must contain at least one item",
                data = (object?)null
            });
        }else{
         var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
         if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
         {
             return Unauthorized(new
             {
                 status = 401,
                 message = "Invalid or missing user identifier",
                 data = (object?)null
             });
         }

        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>(),
            Status = "Pending"
        };


         decimal totalAmount = 0;

         //  Loop through items and create OrderItems
        foreach( var itemRequest in request.Items)
        {
            var menuItem = await _db.MenuItems.FindAsync(itemRequest.ItemId);
            if(menuItem == null)
            {
                return BadRequest(new
                {
                    status = 400,
                    message = $"Menu item with ID {itemRequest.ItemId} not found",
                    data = (object?)null
                });
            }

            var itemTotal = menuItem.Price * itemRequest.Quantity;
            totalAmount += itemTotal;

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ItemId = itemRequest.ItemId,
                Quantity = itemRequest.Quantity,
                Price = menuItem.Price
            };

            _db.OrderItems.Add(orderItem);
        }

        // Update total amount

        order.Total = totalAmount;
        _db.Orders.Add(order);

        await _db.SaveChangesAsync();

        var payment = new Payment
        {
            OrderId = order.Id,
            PaymentMode = request.PaymentMethod,
            PaymentStatus =
                request.PaymentMethod == 1
                    ? "Pending"
                    : "Paid",
            PaidAt =
                request.PaymentMethod == 1
                    ? null
                    : DateTime.Now
        };


        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();


       return Ok(new
       {
             status = 200,
             message = "Order placed successfully",
             data = new
      { order = new
          {
              orderId = order.Id,
              status = order.Status,
              totalAmount = order.Total,
              paymentStatus = payment.PaymentStatus
          },
          }
         });
      }}
}


public class PlaceOrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public int PaymentMethod { get; set; }
}
public class OrderItemRequest
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}


