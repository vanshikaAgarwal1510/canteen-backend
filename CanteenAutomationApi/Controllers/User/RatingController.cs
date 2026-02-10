
using System.Security.Claims;
using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public RatingsController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "User")]
    [HttpPost]
    public IActionResult AddRating(AddRatingRequest request)
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

       
        if (request.Stars < 1 || request.Stars > 5)
            return BadRequest(new
        {
            status = 400,
            message = "Rating must be between 1 and 5 stars",
            data = (object?)null
        });

       
        var order = _db.Orders
            .FirstOrDefault(o => o.Id == request.OrderId && o.UserId == userId);

        if (order == null)
            return BadRequest(new
            {
                status = 400,
                message = "Order not found",
                data = (object?)null
            });

       
        if (order.Status != "Completed")
            return BadRequest(new
            {
                status = 400,
                message = "You can only rate items from completed orders",
                data = (object?)null
            });

       
        var itemOrdered = _db.OrderItems
            .Any(oi => oi.OrderId == request.OrderId && oi.ItemId == request.ItemId);

        if (!itemOrdered)
            return BadRequest(new
            {
                status = 400,
                message = "You can only rate items that were part of your order",
                data = (object?)null
            });

                var rating = _db.Ratings.FirstOrDefault(r =>
                r.OrderId == request.OrderId &&
                r.MenuItemId == request.ItemId &&
                r.UserId == userId
            );

            if (rating != null)
            {
                rating.Stars = request.Stars;
                _db.Ratings.Update(rating);
             }
             else
             {
                var ratingNew = new Rating
             {
                 UserId = userId,
                 MenuItemId = request.ItemId, 
                 OrderId = request.OrderId,
                 Stars = request.Stars,
                 CreatedAt=DateTime.UtcNow
             };
            _db.Ratings.Add(ratingNew);
             }   
        _db.SaveChanges();

        return Ok(new
        {
            status = 200,
            message = "Rating submitted successfully",
            data = (object?)null
        });
    }
}
public class AddRatingRequest{
    public int OrderId { get; set; }
    public int ItemId { get; set; }
    public int Stars { get; set; }  
   
}