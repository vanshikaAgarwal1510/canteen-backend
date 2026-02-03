namespace CanteenBackend.Models;

public class Order
{
    public int Id { get; set; }
    public required string Status { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }

    // Foreign key
    public required int UserId { get; set; }

    // Navigation
    public  User User { get; set; }= null!;
    public required List<OrderItem> Items { get; set; }
}
