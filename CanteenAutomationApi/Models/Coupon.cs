using System;
using System.ComponentModel.DataAnnotations;

public class Coupon
{
    [Key]
    public int CouponId { get; set; }

  
    [MaxLength(20)]
    public required string Code { get; set; }   // e.g. WELCOME50

    [MaxLength(10)]
    public required string DiscountType { get; set; } // FLAT or PERCENT
    public required decimal DiscountValue { get; set; } // 50 or 10

    public required decimal MinOrderAmount { get; set; }
    public required DateTime ExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}