
namespace CanteenBackend.Models
{
public class Settings
{
    public int Id { get; set; }

    public string CanteenName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;

    // existing
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public bool IsOpen { get; set; }

    
    public bool IsOnlineOrderingEnabled { get; set; }
    public bool RequirePickupCode { get; set; }
    public int MaxActiveOrders { get; set; }

    public DateTime UpdatedAt { get; set; }
}}