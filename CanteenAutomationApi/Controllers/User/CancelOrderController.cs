
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CanteenBackend.Data;
using CanteenBackend.Models;

[Authorize(Roles = "User")]
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
    public async Task<IActionResult> CancelOrder(
        CancelOrderRequest request)
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

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new
            {
                status = 401,
                message = "Invalid or missing user ID",
                data = (object?)null
            });
        }

        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId);

        if (order == null)
        {
            return NotFound(new
            {
                status = 404,
                message = "No order found",
                data = (object?)null
            });
        }

        if (order.UserId != userId)
        {
            return StatusCode(403, new
            {
                status = 403,
                message = "You cannot cancel someone else's order",
                data = (object?)null
            });
        }

        if (order.Status != 1)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Only pending orders can be cancelled",
                data = (object?)null
            });
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        try
        {
            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.OrderId == order.Id);

            // Refund eligible non-wallet payments to the wallet.
            if (payment != null && payment.PaymentMode != 1)
            {
                var user = await _db.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new
                    {
                        status = 404,
                        message = "User not found",
                        data = (object?)null
                    });
                }

                user.WalletBalance += order.FinalAmount;
                payment.PaymentStatus = "Refunded";

                _db.WalletTransactions.Add(new WalletTransaction
                {
                    UserId = user.Id,
                    Amount = order.FinalAmount,
                    Type = "Credit",
                    Description =
                        $"Refund for cancelled Order #{order.Id}",
                    Status = "Success",
                    ReferenceId = payment.Id.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
            }

            order.Status = 5;

            await _db.SaveChangesAsync();
            
            await transaction.CommitAsync();

            return Ok(new
            {
                status = 200,
                message = "Order cancelled successfully",
                data = new
                {
                    OrderId = order.Id,
                    Status = order.Status
                }
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

public class CancelOrderRequest
{
    public int OrderId { get; set; }
    public required string ApiKey { get; set; }
}
