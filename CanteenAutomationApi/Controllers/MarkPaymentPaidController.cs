using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


[Authorize(Roles ="Admin,Staff")]
[ApiController]
[Route("api/mark-payment-paid")]
public class MarkPaymentPaidController : ControllerBase
{
    private readonly AppDbContext _db;
    public MarkPaymentPaidController(AppDbContext db)
    {
        _db = db;
    }
    [HttpPut]
    public IActionResult UpdatePaymentStatus(int orderId)

    {
       var payment =  _db.Payments.FirstOrDefault(o => o.Id == orderId);
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


