
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


[Authorize(Roles ="Admin")]
[ApiController]
[Route("api/update-availability")]
public class UpdateAvailabilityController : ControllerBase
{
    private readonly AppDbContext _db;
    public UpdateAvailabilityController(AppDbContext db)
    {
        _db = db;
    }
    [HttpPut]
    public IActionResult UpdateAvailability(UpdateAvailabilityRequest request){
         var item =  _db.MenuItems.FirstOrDefault(i => i.Id == request.ItemId);
         if(item == null)
         {
        return NotFound(new
        {
            status = 404,
            message = "Item not found",
            data = (object?)null
        });
            }
            item.IsAvailable = request.IsAvailable;
            _db.SaveChanges();
            
        return Ok(new
        {
            status = 200,
            message = "Availability updated successfully",
            data =  new{   
                ItemId = item.Id,
                IsAvailable = item.IsAvailable
        }
        });
    }

}
public class UpdateAvailabilityRequest
{
    public int ItemId { get; set; }
    public bool IsAvailable { get; set; }
}


