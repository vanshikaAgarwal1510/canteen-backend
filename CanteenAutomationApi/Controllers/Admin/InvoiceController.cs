using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CanteenBackend.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

[ApiController]
[Route("api/invoice")]
public class InvoiceController : ControllerBase
{
     private readonly AppDbContext _db;

    public InvoiceController(AppDbContext db)
    {
        _db = db;
    }
  
 [Authorize]
[HttpPost("get-invoice")]
public async Task<IActionResult> GetInvoice(
    [FromBody] InvoiceRequestDto request)
{
    if (request.ApiKey != Constants.api)
    {
        return Unauthorized(new
        {
            status = 401,
            message = "An invalid API key was provided",
            data = (object?)null
        });
    }
var order = await _db.Orders
    .Include(x => x.User)
    .Include(x => x.Items)
        .ThenInclude(x => x.Item)
    .FirstOrDefaultAsync(x => x.Id == request.OrderId);

    if (order == null)
    {
        return NotFound(new
        {
            status = 404,
            message = "Order not found",
            data = (object?)null
        });
    }
    

 Console.WriteLine("id: " + order.Id);
Console.WriteLine($"User: {order.User?.FullName ?? "NULL"}");
Console.WriteLine($"Items: {order.Items?.Count ?? 0}");

    var document = new InvoiceDocument(order);

    byte[] pdf = document.GeneratePdf();

    return File(
        pdf,
        "application/pdf",
        $"Invoice-{order.Id}.pdf"
    );
}
public class InvoiceRequestDto
{
    public required string ApiKey { get; set; }
    public int OrderId { get; set; } 
}
}



 