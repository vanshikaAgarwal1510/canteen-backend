using Microsoft.EntityFrameworkCore;
using CanteenBackend.Models;

namespace CanteenBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

          public DbSet<Role> Roles { get; set; }
          public DbSet<User> Users { get; set; }
          public DbSet<MenuCategory> MenuCategories { get; set; }
          public DbSet<MenuItem> MenuItems { get; set; }
          public DbSet<Order> Orders { get; set; }
          public DbSet<OrderItem> OrderItems { get; set; }
          public DbSet<Payment> Payments { get; set; }
    }
}
