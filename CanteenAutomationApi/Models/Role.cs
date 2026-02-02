namespace CanteenBackend.Models;

public class Role
{
    public int Id { get; set; }
    public required string Name { get; set; }

    // Navigation
    public  List<User>? Users { get; set; }
}
    