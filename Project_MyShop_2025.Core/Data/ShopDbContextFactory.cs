using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Project_MyShop_2025.Core.Data
{
    public class ShopDbContextFactory : IDesignTimeDbContextFactory<ShopDbContext>
    {
        public ShopDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ShopDbContext>();
            var connectionString = DatabasePathHelper.GetConnectionString();
            optionsBuilder.UseSqlite(connectionString);

            return new ShopDbContext(optionsBuilder.Options);
        }
    }
}
