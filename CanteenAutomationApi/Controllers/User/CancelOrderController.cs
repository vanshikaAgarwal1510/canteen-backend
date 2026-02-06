using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Claims;


[Authorize(Roles ="User")]
[ApiController]
[Route("api/cancel-order")]
public class CancelOrderController : ControllerBase
{
     private readonly AppDbContext _db;

    public CancelOrderController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> CancelOrder(CancelOrderRequest request )
    {
         var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
         if(string.IsNullOrEmpty(userIdClaim)) 
             return Unauthorized(new
             {
                 status = 401,
                 message = "Invalid or missing user ID",
                 data = (object?)null
             });
         
         int userId = int.Parse(userIdClaim);
         var order = _db.Orders.Find(request.OrderId);
 
            if(order == null)
         {
            return NotFound(new
            {
                status = 404,
                message = "No orders found to cancel",
                data = (object?)null
            });}

            if(order.UserId != userId)
            {
              return Unauthorized(new
              {
                 status = 403,
                 message = "You cannot cancel someone else's order",
                 data = (object?)null
            });
            }
            
            if(order.Status != "Pending")
            {
              return BadRequest(new
              {
                 status = 400,
                 message = "Only pending orders can be cancelled",
                 data = (object?)null
            });
            }

           var payment = _db.Payments.Where(p => p.OrderId == order.Id).FirstOrDefault();

           if(payment !=null && payment.PaymentMode != 1) 
             {
               var user = _db.Users.Find(userId);
               if(user != null)
               {
                   user.WalletBalance += order.FinalAmount;
                   payment.PaymentStatus = "Refunded";
               }
         
             }
        
            order.Status = "Cancelled";
            await _db.SaveChangesAsync();


               return Ok(new
               {
                   status = 200,
                   message = "Order cancelled successfully",
                   data = new
                   {
                          OrderId = order.Id,
                          status = order.Status
                   }
               });
    }
}
public class CancelOrderRequest
{
    public int OrderId { get; set; }
}
