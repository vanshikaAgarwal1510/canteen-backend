// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using CanteenBackend.Data;
// using Microsoft.EntityFrameworkCore;
// using System.Threading.Tasks;


// [Authorize(Roles ="Admin,Staff")]
// [ApiController]
// [Route("api/orders")]
// public class GetOrdersController : ControllerBase
// {
//      private readonly AppDbContext _db;

//     public GetOrdersController(AppDbContext db)
//     {
//         _db = db;
//     }
//     [HttpGet]
//     public async Task<IActionResult> GetOrders()
//     {
//         var orders = await _db.Orders
//             .Include(o => o.User)
//             .Include(o => o.OrderItems)
//                 .ThenInclude(oi => oi.Item)
//             .OrderByDescending(o => o.OrderDate)
//             .Select(o => new OrderResponseDto
//             {
//                 OrderId = o.OrderId,
//                 OrderDate = o.OrderDate,
//                 Status = o.Status,
//                 TotalAmount = o.TotalAmount,

//                 UserId = o.User.Id,
//                 UserName = o.User.Name,

//                 Items = o.OrderItems.Select(oi => new OrderItemDto
//                 {
//                     ItemId = oi.ItemId,
//                     ItemName = oi.Item.Name,
//                     Quantity = oi.Quantity,
//                     Price = oi.Price
//                 }).ToList()
//             })
//             .ToListAsync();

//         return Ok(new
//         {
//             status = 200,
//             message = "Orders fetched successfully",
//             data = orders
//         });
//     }
// }
