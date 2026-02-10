using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


[Authorize(Roles ="Admin,Staff")]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db)
    {
        _db = db;
    }
    [HttpPost("update-status")]
     public IActionResult UpdateOrderStatus(UpdateOrderRequest request)
{
    var order = _db.Orders.FirstOrDefault(o => o.Id == request.OrderId);
    if (order == null)
    {
        return NotFound(new
        {
            status = 404,
            message = "Order not found",
            data = (object?)null
        });
    }

    var validTransitions = new Dictionary<string, List<string>>
    {
        { "Pending", new List<string> { "Preparing" } },
        { "Preparing", new List<string> { "Ready" } },
        { "Ready", new List<string> { "Completed" } },
        { "Completed", new List<string>() }
    };

    if (!validTransitions.ContainsKey(order.Status) ||
        !validTransitions[order.Status].Contains(request.NewStatus))
    {
        return BadRequest(new
        {
            status = 400,
            message = "Invalid status transition",
            data = (object?)null
        });
    }

   
    if (request.NewStatus == "Completed")
    {
        // Payment check
        var payment = _db.Payments.FirstOrDefault(p => p.OrderId == order.Id);
        if (payment == null || payment.PaymentStatus != "Paid")
        {
            return BadRequest(new
            {
                status = 400,
                message = "Payment pending. Order cannot be completed.",
                data = (object?)null
            });
        }

        // Pickup code check
        if (order.IsPickedUp)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Order already completed",
                data = (object?)null
            });
        }

        if (string.IsNullOrEmpty(request.PickupCode) ||
            order.PickupCode != request.PickupCode)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Invalid pickup code",
                data = (object?)null
            });
        }

        order.IsPickedUp = true; 
    }

    order.Status = request.NewStatus;
    _db.SaveChanges();

    return Ok(new
    {
        status = 200,
        message = "Order status updated successfully",
        data = new
        {
            OrderId = order.Id,
            NewStatus = order.Status
        }
    });
}
       
    [HttpPost("mark-payment-paid")]
    public IActionResult UpdatePaymentStatus([FromBody] UpdatePaymentStatusRequest request)
{
    var payment = _db.Payments
        .FirstOrDefault(p => p.OrderId == request.OrderId);

        Console.WriteLine(payment);

    if (payment == null)
    {
        return NotFound(new
        {
            status = 404,
            message = "Payment record not found",
            data = (object?)null
        });
    }

    if (payment.PaymentStatus == "Paid")
    {
        return BadRequest(new
        {
            status = 400,
            message = "Payment is already marked as paid",
            data = (object?)null
        });
    }

    var order = _db.Orders.FirstOrDefault(o => o.Id == request.OrderId);
    if (order == null)
    {
        return BadRequest(new
        {
            status = 400,
            message = "Order not found",
            data = (object?)null
        });
    }

    // Optional business rule
    if (order.Status == "Completed")
    {
        return BadRequest(new
        {
            status = 400,
            message = "Cannot update payment for completed order",
            data = (object?)null
        });
    }

    // Prevent wallet double-payment
    if (payment.PaymentMode == 5) // Wallet
    {
        return BadRequest(new
        {
            status = 400,
            message = "Wallet payments are auto-settled",
            data = (object?)null
        });
    }

    payment.PaymentStatus = "Paid";
    payment.PaidAt = DateTime.UtcNow;

    _db.SaveChanges();

    return Ok(new
    {
        status = 200,
        message = "Payment status updated successfully",
        data = new
        {
            paymentId = payment.Id,
            newStatus = payment.PaymentStatus
        }
    });
}
    }
    public class UpdatePaymentStatusRequest
{
    public int OrderId { get; set; }
}
    

