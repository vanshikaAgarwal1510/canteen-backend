public class PaymentMethod
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}