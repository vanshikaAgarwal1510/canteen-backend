namespace CanteenBackend.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    // Foreign keys
    public int OrderId { get; set; }
    public int ItemId { get; set; }

    // Navigation
    public required Order Order { get; set; }
    public required MenuItem Item { get; set; }
}
