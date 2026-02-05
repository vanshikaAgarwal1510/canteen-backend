namespace CanteenBackend.Models;

public class MenuItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Foreign key
    public int CategoryId { get; set; }

    // Navigation
    public required MenuCategory Category { get; set; }
    public  List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
