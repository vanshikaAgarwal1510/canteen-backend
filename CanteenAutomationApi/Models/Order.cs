namespace CanteenBackend.Models;

public class Order
{
    public int Id { get; set; }
    public required string Status { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }

    public int OrderType{get; set;} // 1= Dine-In, 2= Takeaway,
    

    // Foreign key
    public required int UserId { get; set; }

    // Navigation
    public  User User { get; set; }= null!;
    public  List<OrderItem> Items { get; set; } = new();
     public Payment? Payment { get; set; }
}
