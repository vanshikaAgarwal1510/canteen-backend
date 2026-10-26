
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
    public async Task<IActionResult> GetData(DashboardDataRequest request)
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

        // India Standard Time
        var indiaTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        var indiaNow = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow, indiaTimeZone);

        // Today's boundaries in IST, converted to UTC for PostgreSQL
        var indiaStart = indiaNow.Date;
        var indiaEnd = indiaStart.AddDays(1);

        var todayStartUtc = TimeZoneInfo.ConvertTimeToUtc(
            indiaStart, indiaTimeZone);

        var tomorrowStartUtc = TimeZoneInfo.ConvertTimeToUtc(
            indiaEnd, indiaTimeZone);

        // Today's orders
        var todayOrders = _db.Orders
            .Where(o =>
                o.CreatedAt >= todayStartUtc &&
                o.CreatedAt < tomorrowStartUtc);

        var totalOrders = await todayOrders.CountAsync();

        var cancelledOrders = await todayOrders
            .CountAsync(o => o.Status == 5);
            
        var completedOrders = await todayOrders
            .CountAsync(o => o.Status == 4);

        var todayRevenue = await todayOrders
            .Where(o => o.Status == 4)
            .SumAsync(o => (decimal?)o.FinalAmount) ?? 0m;

        // Top 10 selling items (completed orders only)
        var items = await _db.OrderItems
            .Where(oi =>
                oi.Item != null &&
                oi.Order != null &&
                oi.Order.Status == 4)
            .GroupBy(oi => new
            {
                oi.ItemId,
                ItemName = oi.Item!.Name
            })
            .Select(g => new
            {
                itemId = g.Key.ItemId,
                itemName = g.Key.ItemName,
                quantity = g.Sum(x => x.Quantity),
                revenue = g.Sum(x => x.Quantity * x.Price)
            })
            .OrderByDescending(x => x.quantity)
            .Take(10)
            .ToListAsync();

        // Current month's boundaries in IST, converted to UTC
        var monthStartIndia = new DateTime(
            indiaNow.Year,
            indiaNow.Month,
            1,
            0, 0, 0,
            DateTimeKind.Unspecified);

        var nextMonthIndia = monthStartIndia.AddMonths(1);

        var monthStartUtc = TimeZoneInfo.ConvertTimeToUtc(
            monthStartIndia, indiaTimeZone);

        var nextMonthStartUtc = TimeZoneInfo.ConvertTimeToUtc(
            nextMonthIndia, indiaTimeZone);

        // Completed orders for the current Indian calendar month
        var monthlyOrders = _db.Orders
            .Where(o =>
                o.CreatedAt >= monthStartUtc &&
                o.CreatedAt < nextMonthStartUtc &&
                o.Status == 4);

        var totalMonthlyOrders = await monthlyOrders.CountAsync();

        var totalMonthlyRevenue = await monthlyOrders
            .SumAsync(o => (decimal?)o.FinalAmount) ?? 0m;

        // Fetch daily revenue records and group them by Indian date
        var monthlyOrderData = await monthlyOrders
            .Select(o => new
            {
                o.CreatedAt,
                o.FinalAmount
            })
            .ToListAsync();

        var chartData = monthlyOrderData
            .GroupBy(o =>
                TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(o.CreatedAt, DateTimeKind.Utc),
                    indiaTimeZone).Date)
            .Select(g => new
            {
                date = g.Key.ToString("dd MMM"),
                revenue = g.Sum(x => x.FinalAmount)
            })
            .OrderBy(x => DateTime.ParseExact(
                x.date,
                "dd MMM",
                System.Globalization.CultureInfo.InvariantCulture))
            .ToList();
            var orderStatusData = await todayOrders
            .GroupBy(o => o.Status)
            .Select(g => new
            {
                status = g.Key,
                count = g.Count()
            })
            .ToListAsync();


        return Ok(new
        {
            status = 200,
            message = "Dashboard data fetched successfully",
            data = new DashboardDataResponse
            {
                TotalOrdersToday = totalOrders,
                CancelledOrdersToday = cancelledOrders,
                RevenueToday = todayRevenue,
                CompletedOrdersToday = completedOrders,
                TopSellingItems = items,
                TotalMonthlyOrders = totalMonthlyOrders,
                TotalMonthlyRevenue = totalMonthlyRevenue,
                DailyRevenueChartData = chartData,
                OrderStatusData = orderStatusData
            }
        });
    }
}

public class DashboardDataRequest
{
    public required string ApiKey { get; set; }
}

public class DashboardDataResponse
{
    public int TotalOrdersToday { get; set; }
    public int CancelledOrdersToday { get; set; }
    public int CompletedOrdersToday{get; set;}
    public decimal RevenueToday { get; set; }
    public object TopSellingItems { get; set; } = null!;
    public int TotalMonthlyOrders { get; set; }
    public decimal TotalMonthlyRevenue { get; set; }
    public object DailyRevenueChartData { get; set; } = null!;
    public object OrderStatusData { get; set; } = null!;
}
