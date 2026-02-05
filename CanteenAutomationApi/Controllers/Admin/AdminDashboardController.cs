using CanteenBackend.Data;
using CanteenBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin-dashboard")]
public class AdminDashboardController : ControllerBase
{
 private readonly AppDbContext _db;
   public AdminDashboardController(AppDbContext db)
    {
        _db = db;
    }

   
    [HttpPost]
     public IActionResult GetData()
    {

    var today = DateTime.Today;

    var todayOrders = _db.Orders
        .Where(o => o.CreatedAt.Date == today);

    var totalOrders = todayOrders.Count();

    var cancelledOrders = todayOrders
        .Count(o => o.Status == "Cancelled");

    var todayRevenue = todayOrders
        .Where(o => o.Status == "Completed")
        .Sum(o => (decimal?)o.Total) ?? 0;

    var items = _db.OrderItems
    .Include(oi => oi.Item)
    .Include(oi => oi.Order)
    .Where(oi =>
        oi.Item != null &&
        oi.Item.Name != null &&
        oi.Order != null &&
        oi.Order.Status == "Completed"
    )
    .GroupBy(oi => oi.Item!.Name)
    .Select(g => new
    {
        itemName = g.Key,
        quantity = g.Sum(x => x.Quantity),
        revenue = g.Sum(x => x.Quantity * x.Price)
    })
    .OrderByDescending(x => x.quantity)
    .Take(10)
    .ToList();


     var monthlyOrders = _db.Orders
        .Where(o =>
            o.CreatedAt.Month == today.Month &&
            o.CreatedAt.Year == today.Year &&
            o.Status == "Completed" 
            );

    var totalMonthlyOrders = monthlyOrders.Count();

    var totalMonthlyRevenue = monthlyOrders
        .Sum(o => (decimal?)o.Total) ?? 0;

      var chartData =monthlyOrders
     .GroupBy(o => o.CreatedAt.Date)
     .Select(g => new
     {
         date = g.Key.ToString("dd MMM"),
         revenue = g.Sum(x => x.Total)
     })
     .OrderBy(x => x.date)
     .ToList();



       
         return Ok(new
         {
              status = 200,
              message = "Dashboard data fetched successfully",
              data = new DashboardDataResponse
              {
                  TotalOrdersToday = totalOrders,
                  CancelledOrdersToday = cancelledOrders,
                  RevenueToday = todayRevenue,
                  TopSellingItems = items,
                  TotalMonthlyOrders = totalMonthlyOrders,
                  TotalMonthlyRevenue = totalMonthlyRevenue,
                  DailyRevenueChartData = chartData
                }
         }); 
          }
}

class DashboardDataResponse
{
    public int TotalOrdersToday { get; set; }
    public int CancelledOrdersToday { get; set; }
    public decimal RevenueToday { get; set; }
    public object TopSellingItems { get; set; } = null!;
    public int TotalMonthlyOrders { get; set; }
    public decimal TotalMonthlyRevenue { get; set; }
    public object DailyRevenueChartData { get; set; } = null!;
}
