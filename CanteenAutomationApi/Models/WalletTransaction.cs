
namespace CanteenBackend.Models
{
    public class WalletTransaction
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        // Positive transaction amount
        public decimal Amount { get; set; }

        // "Credit" or "Debit"
        public string Type { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        // "Success", "Pending", or "Failed"
        public string Status { get; set; } = "Success";

        // Optional payment/order reference
        public string? ReferenceId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
