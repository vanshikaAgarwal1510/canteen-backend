namespace CanteenBackend.Models;

public class MenuCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Navigation
    public required List<MenuItem> Items { get; set; }
}
