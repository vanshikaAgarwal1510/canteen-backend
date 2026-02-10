using System.Security.Claims;
using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/place-order")]
[ApiController]
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
        if (request.Items == null || !request.Items.Any())
        {
            return BadRequest(new
            {
                status = 400,
                message = "Order must contain at least one item",
                data = (object?)null
            });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new
            {
                status = 401,
                message = "Invalid user",
                data = (object?)null
            });
        }

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return Unauthorized(new
            {
                status = 401,
                message = "User does not exist",
                data = (object?)null
            });
        }

        decimal subTotal = 0;
        decimal discount = 0;

        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // Calculate subtotal
            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                {
                    return BadRequest("Quantity must be greater than zero");
                }

                var menuItem = await _db.MenuItems.FindAsync(item.ItemId);
                if (menuItem == null || !menuItem.IsAvailable)
                {
                    return BadRequest($"Menu item {item.ItemId} is not available");
                }

                subTotal += menuItem.Price * item.Quantity;
            }

            // Coupon logic (ONCE)
            if (!string.IsNullOrEmpty(request.CouponCode))
            {
                var coupon = await _db.Coupons.FirstOrDefaultAsync(c =>
                    c.Code == request.CouponCode.ToUpper() &&
                    c.IsActive &&
                    c.ExpiryDate > DateTime.UtcNow);

                if (coupon == null)
                    return BadRequest("Invalid or expired coupon");

                if (subTotal < coupon.MinOrderAmount)
                    return BadRequest("Order amount too low for this coupon");

                discount = coupon.DiscountType == "FLAT"
                    ? coupon.DiscountValue
                    : subTotal * (coupon.DiscountValue / 100);

                discount = Math.Min(discount, subTotal);
            }

            decimal totalAmount = subTotal - discount;

            // Wallet validation
            if (request.PaymentMethod == 5)
            {
                if (user.WalletBalance < totalAmount)
                {
                    return BadRequest(new
                    {
                        status = 400,
                        message = "Insufficient wallet balance",
                        data = (object?)null
                    });
                }

                user.WalletBalance -= totalAmount;
            }

            //  Create Order
            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending",
                OrderType = request.OrderType,
                SubTotal = subTotal,
                Discount = discount,
                FinalAmount = totalAmount,
                PickupCode = GeneratePickupCode()
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Create Order Items
            foreach (var item in request.Items)
            {
                var menuItem = await _db.MenuItems.FindAsync(item.ItemId);

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ItemId = menuItem!.Id,
                    Quantity = item.Quantity,
                    Price = menuItem.Price
                };

                _db.OrderItems.Add(orderItem);
            }

            await _db.SaveChangesAsync();

            // Payment
            var payment = new Payment
            {
                OrderId = order.Id,
                PaymentMode = request.PaymentMethod,
                PaymentStatus = request.PaymentMethod == 1 ? "Pending" : "Paid",
                PaidAt = request.PaymentMethod == 1 ? null : DateTime.UtcNow
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new
            {
                status = 200,
                message = "Order placed successfully",
                data = new
                {
                    orderId = order.Id,
                    status = order.Status,
                    subTotal = order.SubTotal,
                    discount = order.Discount,
                    totalAmount = order.FinalAmount,
                    paymentStatus = payment.PaymentStatus,
                    pickupCode = order.PickupCode
                }
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private string GeneratePickupCode()
{
    return Random.Shared.Next(1000, 9999).ToString(); // 4-digit
}
}

public class PlaceOrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public int PaymentMethod { get; set; } // 1= cash, 2= phonepay, 3=paytm, 4=googlePay etc.
     public int OrderType{get; set;} // 1= Dine-In, 2= Takeaway,
     public string? CouponCode{get; set;}

}
public class OrderItemRequest
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}

