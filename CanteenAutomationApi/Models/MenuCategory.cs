namespace CanteenBackend.Models;

public class MenuCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public  List<MenuItem> Items { get; set; } = new List<MenuItem>();
}
