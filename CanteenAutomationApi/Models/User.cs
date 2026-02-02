namespace CanteenBackend.Models;

public class User
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }

    // Foreign key
    public int RoleId { get; set; }

    // Navigation
    public required Role Role { get; set; }
    public  List<Order>? Orders { get; set; }
}
