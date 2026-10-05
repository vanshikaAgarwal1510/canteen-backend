using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CanteenBackend.Models;


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
     public async Task<IActionResult> UpdateOrderStatusAsync(UpdateOrderRequest request)
{
  if (request.ApiKey != Constants.api)
    {
        return Unauthorized(new
        {
            status = 401,
            message = "Invalid API key",
            data = (object?)null
        });
    }

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

     var validTransitions = new Dictionary<int, List<int>>
    {
     { 1, new List<int> { 2 } }, // Pending → Preparing
     { 2, new List<int> { 3 } }, // Preparing → Ready
     { 3, new List<int> { 4 } }, // Ready → Completed
     { 4, new List<int>() }       // Completed
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

   
    if (request.NewStatus == 4)
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
                
        if (order.RequirePickupCode)
        {
            if (string.IsNullOrWhiteSpace(request.PickupCode) ||
                order.PickupCode != request.PickupCode.Trim())
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Please enter pickup code to complete the order.",
                    data = (object?)null
                });
            }
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
      if (request.ApiKey != Constants.api)
    {
        return Unauthorized(new
        {
            status = 401,
            message = "Invalid API key",
            data = (object?)null
        });
    }

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
    if (order.Status == 4)
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
            newStatus = payment.PaymentStatus,
            paidAt = payment.PaidAt

        }
    });
}
    
   [HttpPost("validate-pickup-code")]
    public async Task<IActionResult> ValidatePickupCode( [FromBody] ValidatePickupCodeRequest request)
    {
       
        if (request.ApiKey != Constants.api)
        {
            return Unauthorized(new
            {
                status = 401,
                message = "Invalid API key",
                data = (object?)null
            });
        }
    
       
        var order = await _db.Orders
               .FirstOrDefaultAsync(o => o.Id == request.OrderId);
       
           if (order == null)
       {
           return BadRequest(new
           {
               status = 400,
               message = "Order not found",
               data = (object?)null
           });
          }

       
       if (order.Status != 3)
       {
          return BadRequest(new
              {
               status = 400,
               message = "Order is not ready for pickup.",
               data = (object?)null
           });
       }
   
      
     if (order.IsPickedUp)
      {
         return BadRequest(new
          {
               status = 400,
               message = "Order already completed",
             data = (object?)null
          });
     }

  
    var payment = await _db.Payments
        .FirstOrDefaultAsync(p => p.OrderId == order.Id);

    if (payment == null || payment.PaymentStatus != "Paid")
    {
        return BadRequest(new
        {
            status = 400,
            message = "Payment pending. Order cannot be completed.",
            data = (object?)null
        });
    }

    
    if (order.RequirePickupCode)
    {
        if (string.IsNullOrWhiteSpace(request.PickupCode))
        {
            return BadRequest(new
            {
                status = 400,
                message = "Pickup code is required.",
                data = (object?)null
            });
        }

        if (order.PickupCode != request.PickupCode.Trim())
        {
            return BadRequest(new
            {
                status = 400,
                message = "Incorrect Pickup Code",
                data = (object?)null
            });
        }
    }

  
    order.Status = 4;
    order.IsPickedUp = true;

    await _db.SaveChangesAsync();

    return Ok(new
    {
        status = 200,
        message = order.RequirePickupCode
            ? "Pickup code validated successfully."
            : "Order completed successfully.",
        data = new
        {
            status = order.Status,
            isPickedUp = order.IsPickedUp
        }
    });
} 
        }

    public class UpdatePaymentStatusRequest
{
    public required string  ApiKey{get; set;}
    public int OrderId { get; set; }
}
    
    public class ValidatePickupCodeRequest
{
    public required string  ApiKey{get; set;}
    public int OrderId { get; set; }

    public required string PickupCode{get; set;}
}
    
