namespace CanteenBackend.Models;

public class MenuCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Navigation
    public  List<MenuItem> Items { get; set; } = new List<MenuItem>();
}
