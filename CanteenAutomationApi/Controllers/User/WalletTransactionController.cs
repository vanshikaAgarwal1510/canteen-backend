
using System.Security.Claims;
using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/wallet")]
public class WalletController : ControllerBase
{
    private readonly AppDbContext _db;

    public WalletController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "User")]
    [HttpPost("history")]
    public async Task<IActionResult> GetHistory(GetWalletHistoryRequest request)
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
         var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
         if(string.IsNullOrEmpty(userIdClaim)) 
             return Unauthorized(new
             {
                 status = 401,
                 message = "Invalid or missing user ID",
                 data = (object?)null
             });

         int userId = int.Parse(userIdClaim);

        var transactions = await _db.WalletTransactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.Amount,
                    t.Type,
                    t.Description,
                    t.Status,
                    t.ReferenceId,
                    t.CreatedAt
                })
                .ToListAsync();

        return Ok(new
        {
            status = 200,
            message = "Wallet history fetched successfully",
            data = transactions
        });
    }
}
public class GetWalletHistoryRequest{
     public required string ApiKey { get; set; }
}
