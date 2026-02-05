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
    [HttpPut("update-status")]
    public IActionResult UpdateOrderStatus(UpdateOrderRequest request)

    {
       var order =  _db.Orders.FirstOrDefault(o => o.Id == request.OrderId);
       if(order == null)
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
    
    if (!validTransitions.ContainsKey(order.Status) || !validTransitions[order.Status].Contains(request.NewStatus))
    {
        return BadRequest(new
        {
            status = 400,
            message = "Invalid status transition",
            data = (object?)null
        });
    }
        // If marking as Completed, ensure payment is done
    if(request.NewStatus == "Completed")
        {
            var payment = _db.Payments.FirstOrDefault(p => p.OrderId == order.Id);

            if(payment != null && payment.PaymentStatus != "Paid")
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Payment pending. Order cannot be completed.",
                    data = (object?)null
                });
            }
        }

         order.Status = request.NewStatus;
            _db.SaveChanges();


        return Ok(new
        {
            status = 200,
            message = "Orders updated successfully",
            data =  new{   
                OrderId = order.Id,
                NewStatus = order.Status
        }
        });
    }
   
    [HttpPut("mark-payment-paid")]
     public IActionResult UpdatePaymentStatus(int orderId)

    {
       var payment =  _db.Payments.FirstOrDefault(o => o.OrderId == orderId);
       if(payment == null)
       {
        return NotFound(new
        {
            status = 404,
            message = "Payment record not found",
            data = (object?)null
        });
       }

     if( payment.PaymentStatus == "Paid"){
        return BadRequest(new
        {
            status = 400,
            message = "Payment is already marked as paid",
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
             data =  new{   
                PaymentId = payment.Id,
                NewStatus = payment.PaymentStatus
        }
        });
    }


}


