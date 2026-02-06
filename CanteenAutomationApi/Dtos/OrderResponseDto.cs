using System;
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }= null!;
        public decimal TotalAmount { get; set; }

        public decimal Discount { get; set; }
        public decimal SubTotal { get; set; }
        public string? PaymentStatus { get; set; }
        public int OrderType { get; set; }

        // User info
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;

        // Items
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

public class OrderItemDto
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class UpdateOrderRequest
{
    public int OrderId { get; set; }
    public string NewStatus { get; set; } = null!;
}


