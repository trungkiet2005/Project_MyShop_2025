using Microsoft.EntityFrameworkCore;
using Project_MyShop_2025.Core.Models;

namespace Project_MyShop_2025.Core.Data
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Promotion> Promotions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Promotion: unique code index
            modelBuilder.Entity<Promotion>()
                .HasIndex(p => p.Code)
                .IsUnique();

            // Customer: index on phone for quick lookup
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Phone);

            // Order -> Customer relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Order -> Promotion relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Promotion)
                .WithMany()
                .HasForeignKey(o => o.PromotionId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Categories sẽ được seed từ DbSeeder, không dùng HasData để tránh conflict
        }
    }
}
