using System.ComponentModel.DataAnnotations;
using CanteenBackend.Models;
public class Rating
{
    public int RatingId { get; set; }

    public int UserId { get; set; }
    public int MenuItemId { get; set; }
    public int OrderId { get; set; }

    [Range(1, 5)]
    public int Stars { get; set; }   // 1 to 5

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties (optional but good)
    public User User { get; set; }  = null!;
    public MenuItem MenuItem { get; set; } = null!;
    public Order Order { get; set; } = null!;
}