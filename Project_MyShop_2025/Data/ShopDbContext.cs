using Microsoft.EntityFrameworkCore;

namespace Project_MyShop_2025.Data
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
        {
        }

        // Add DbSet properties here later
    }
}
