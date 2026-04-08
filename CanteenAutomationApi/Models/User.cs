namespace CanteenBackend.Models;


public class User
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public string? ImageUrl { get; set; }
    public string? Email { get; set; }        // nullable for normal users
    public string? PasswordHash { get; set; } // only for admin/staff

    public string? MobileNumber { get; set; }

    public string? OtpCode { get; set; }
    public DateTime? OtpExpiry { get; set; }

     public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   public int? StaffType { get; set; } //1 fullTime, 2 part-time, 3 contract

    public bool IsActive { get; set; }

    public decimal WalletBalance { get; set; } = 0;

    public bool IsUniversityStudent { get; set; } = false;
    // Foreign key
    public int RoleId { get; set; }

    // Navigation
    public  Role Role { get; set; }= null!;
    public  List<Order>? Orders { get; set; }
}

