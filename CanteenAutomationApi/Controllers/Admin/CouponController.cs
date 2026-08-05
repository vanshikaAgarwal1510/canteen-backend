using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using CanteenBackend.Data;
using CanteenBackend.Models;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/coupon")]
public class CouponController : ControllerBase
{
    private readonly AppDbContext _db;

    public CouponController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("get-coupon")]
    public async Task<IActionResult> GetCouponList(GetCouponRequest request)
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
        var coupon = await _db.Coupons
            .Where(u =>!u.IsDeleted)
            .Select(u => new AddCouponResponse
            {
                Id = u.CouponId,
                Code = u.Code,
                DiscountType = u.DiscountType,
                DiscountValue = u.DiscountValue,
                ExpiryDate = u.ExpiryDate,
                MinOrderAmount = u.MinOrderAmount,
                IsActive = u.IsActive
            })
            .ToListAsync();

        if (coupon.Count == 0)
            return NotFound(new
            {
                status = 404,
                message = "Coupon not found",
                data = (object?)null
            });

        return Ok(new
        {
            status = 200,
            message = "Coupon retrieved successfully",
            data = coupon
        });
    }

    [HttpPost("add-coupon")]
    public async Task<IActionResult> AddCoupon(AddCouponRequest request)
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
        var existingCoupon = await _db.Coupons
            .AnyAsync(u => u.Code.ToLower() == request.Code.ToLower() && !u.IsDeleted);

        if (existingCoupon)
            return BadRequest(new
            {
                status = 400,
                message = "Coupon already exists",
                data = (object?)null
            });


        if (request.ExpiryDate <= DateTime.UtcNow)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Expiry date must be in future",
                data = (object?)null
            });
        }
        // Common validation
        if (request.DiscountValue <= 0)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Discount must be greater than 0"
            });
        }

        // Type-based validation
        if (request.DiscountType == "Percentage" && request.DiscountValue > 100)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Percentage discount cannot exceed 100%"
            });
        }


        var coupon = new Coupon
        {
            Code = request.Code.ToUpper().Trim(),
            DiscountType = request.DiscountType,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            DiscountValue = request.DiscountValue,
            ExpiryDate = request.ExpiryDate,
            MinOrderAmount = request.MinOrderAmount
        };



        _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            status = 200,
            message = "Coupon added successfully",
            data = new AddCouponResponse
            {
                Id = coupon.CouponId,
                Code = coupon.Code,
                DiscountType = coupon.DiscountType,
                DiscountValue = coupon.DiscountValue,
                ExpiryDate = coupon.ExpiryDate,
                MinOrderAmount = coupon.MinOrderAmount,
                IsActive = coupon.IsActive

            }

        });
    }
    [HttpPost("update-coupon")]
    public async Task<IActionResult> UpdateCoupon(UpdateCouponRequest request)
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

        var coupon = await _db.Coupons
            .FirstOrDefaultAsync(u => u.CouponId == request.Id );

        if (coupon == null|| coupon.IsDeleted)
            return BadRequest(new
            {
                status = 400,
                message = "Coupon does not exist",
                data = (object?)null
            });

         

        var couponExists = await _db.Coupons
        .AnyAsync(u => u.Code.ToLower() == request.Code.ToLower() && u.CouponId != request.Id);

        if (couponExists)
        {
            return BadRequest(new
            {   
                status = 400,
                message = "coupon already in use",
                data = (object?)null
            });
        }

        if (request.ExpiryDate <= DateTime.UtcNow)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Expiry date must be in future",
                data = (object?)null
            });
        }
        // Common validation
        if (request.DiscountValue <= 0)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Discount must be greater than 0"
            });
        }

        // Type-based validation
        if (request.DiscountType == "Percentage" && request.DiscountValue > 100)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Percentage discount cannot exceed 100%"
            });
        }
        coupon.Code = request.Code.ToUpper().Trim();
        coupon.DiscountType = request.DiscountType;
        coupon.IsActive = request.IsActive;
        coupon.DiscountValue = request.DiscountValue;
        coupon.ExpiryDate = request.ExpiryDate;
        coupon.MinOrderAmount = request.MinOrderAmount;



        await _db.SaveChangesAsync();

        return Ok(new
        {
            status = 200,
            message = "Coupon updated successfully",
            data = new AddCouponResponse
            {
                Id = coupon.CouponId,
                Code = coupon.Code,
                DiscountType = coupon.DiscountType,
                DiscountValue = coupon.DiscountValue,
                ExpiryDate = coupon.ExpiryDate,
                MinOrderAmount = coupon.MinOrderAmount,
                IsActive = coupon.IsActive
            }
        });
    }

    [HttpPost("delete-coupon")]
    public async Task<IActionResult> DeleteCoupon([FromBody] DeleteCouponRequest request)
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
        var coupon = await _db.Coupons
       .FirstOrDefaultAsync(u => u.CouponId == request.Id);

        if (coupon == null)
            return NotFound(new
            {
                status = 404,
                message = "Coupon not found",
                data = (object?)null
            });




        coupon.IsDeleted = true;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            status = 200,
            message = "Coupon deactivated successfully",
            data = (object?)null
        });
    }

}
public class GetCouponRequest
{
    public required string ApiKey { get; set; }
}
public class AddCouponRequest
{
    public required string ApiKey { get; set; }
    public required string Code { get; set; }
    public required string DiscountType { get; set; }
    public required decimal DiscountValue { get; set; }
    public required decimal MinOrderAmount { get; set; }
    public required DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
}

public class AddCouponResponse
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string DiscountType { get; set; }
    public required decimal DiscountValue { get; set; }
    public required decimal MinOrderAmount { get; set; }
    public required DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateCouponRequest
{
    public required string ApiKey { get; set; }
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string DiscountType { get; set; }
    public required decimal DiscountValue { get; set; }
    public required decimal MinOrderAmount { get; set; }
    public required DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }

}

public class DeleteCouponRequest
{
    public required string ApiKey { get; set; }
    public int Id { get; set; }
}