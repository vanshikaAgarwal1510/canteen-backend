namespace CanteenBackend.Models;


public class User
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }

   public int? StaffType { get; set; } //1 fullTime, 2 part-time, 3 contract

    public bool IsActive { get; set; }

    public decimal WalletBalance { get; set; } = 0;


    // Foreign key
    public int RoleId { get; set; }

    // Navigation
    public  Role Role { get; set; }= null!;
    public  List<Order>? Orders { get; set; }
}

