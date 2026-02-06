using System.Security.Claims;
using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



[Route("api/coupons")]
public class CouponController : ControllerBase
{
 private readonly AppDbContext _db;
   public CouponController(AppDbContext db)
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
            message = "Invalid or missing user identifier",
            data = (object?)null
        });
    }

    
    var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
    if (!userExists)
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

 foreach (var itemRequest in request.Items)
    {
        var menuItem = await _db.MenuItems.FindAsync(itemRequest.ItemId);
        if (menuItem == null)
        {
            return BadRequest(new
            {
                status = 400,
                message = $"Menu item with ID {itemRequest.ItemId} not found",
                data = (object?)null
            });
        }

        if (!menuItem.IsAvailable)
        {
            return BadRequest(new
            {
                status = 400,
                message = $"Menu item {menuItem.Name} is not available",
                data = (object?)null
            });
        }

        var itemTotal = menuItem.Price * itemRequest.Quantity;
        totalAmount += itemTotal;
    }
        
        if (!string.IsNullOrEmpty(request.CouponCode))
        {
            var coupon = _db.Coupons
                .FirstOrDefault(c =>
                    c.Code == request.CouponCode &&
                    c.IsActive &&
                    c.ExpiryDate > DateTime.UtcNow);
        
            if (coupon == null)
                return BadRequest("Invalid or expired coupon");
        
            if (totalAmount < coupon.MinOrderAmount)
                return BadRequest("Order amount too low for this coupon");
        
            if (coupon.DiscountType == "FLAT")
                discount = coupon.DiscountValue;
            else if (coupon.DiscountType == "PERCENT")
                discount = totalAmount * (coupon.DiscountValue / 100);
        }
            var total = totalAmount - discount;
        


    return Ok(new
    {
        status = 200,
        message = "Coupon applied successfully",
        data = new ValidateCouponResponse
        {
            TotalAmount = totalAmount,
            Discount = discount,
            FinalAmount = total,
            CouponCode = request.CouponCode
        }
    });
}}



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