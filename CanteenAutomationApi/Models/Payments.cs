namespace CanteenBackend.Models;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int PaymentMode { get; set; } // e.g., 1 = counter, 2 = phonepay, 3 =paytm, 4=googlePay etc.
    public string PaymentStatus { get; set; } = null!; // e.g., "Pending", "Paid",
    public DateTime? PaidAt { get; set; }

    // Navigation
    public Order? Order { get; set; }
}

