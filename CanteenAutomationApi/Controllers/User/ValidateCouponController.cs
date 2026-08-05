using System.Security.Claims;
using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



[Route("api/coupons")]
public class VadilateCouponController : ControllerBase
{
 private readonly AppDbContext _db;
   public VadilateCouponController(AppDbContext db)
    {
        _db = db;
    }
[Authorize(Roles = "User")]
[HttpPost]
public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
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
    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
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

    decimal totalAmount = 0;
    decimal discount = 0;

    foreach (var item in request.Items)
    {
        if (item.Quantity <= 0)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Quantity must be greater than zero",
                data = (object?)null
            });
        }

        var menuItem = await _db.MenuItems.FindAsync(item.ItemId);

        if (menuItem == null || !menuItem.IsAvailable)
        {
            return BadRequest(new
            {
                status = 400,
                message = $"Menu item {item.ItemId} is not available",
                data = (object?)null
            });
        }

        totalAmount += menuItem.Price * item.Quantity;
    }

    //  Coupon logic
    if (!string.IsNullOrWhiteSpace(request.CouponCode))
    {
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c =>
            c.Code.ToLower() == request.CouponCode.ToLower() &&
            c.IsActive &&
            c.ExpiryDate > DateTime.UtcNow);

        if (coupon == null)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Invalid or expired coupon",
                data = (object?)null
            });
        }

        if (coupon.CouponId == 2 && !user.IsUniversityStudent)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Coupon not valid for you",
                data = (object?)null
            });
        }

        if (totalAmount < coupon.MinOrderAmount)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Order amount too low for this coupon",
                data = (object?)null
            });
        }

        discount = coupon.DiscountType == "FLAT"
            ? coupon.DiscountValue
            : totalAmount * (coupon.DiscountValue / 100);

        //  Prevent over-discount
        discount = Math.Min(discount, totalAmount);
    }

    var finalAmount = totalAmount - discount;

    return Ok(new
    {
        status = 200,
        message = "Coupon applied successfully",
        data = new ValidateCouponResponse
        {
            TotalAmount = totalAmount,
            Discount = discount,
            FinalAmount = finalAmount,
            CouponCode = request.CouponCode
        }
    });
}


public class OrderItemsRequest
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}


public class ValidateCouponRequest
{
    public List<OrderItemsRequest> Items { get; set; } = new();
    public string CouponCode { get; set; } = null!;
}

class ValidateCouponResponse
{
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal FinalAmount { get; set; }
    public  String CouponCode { get; set; }= null!;
}
}