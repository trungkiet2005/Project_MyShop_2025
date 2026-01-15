using Microsoft.EntityFrameworkCore;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Models;
using Project_MyShop_2025.Core.Services.Implementations;
using Project_MyShop_2025.Core.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Data.Sqlite;

namespace Project_MyShop_2025.Tests
{
    public class ProductServiceTests
    {
        private ShopDbContext CreateDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<ShopDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new ShopDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task SearchProductsAsync_ReturnsAll_WhenSearchIsEmpty()
        {
            // Arrange
            using var context = CreateDbContext();
            var category = new Category { Name = "Fruits", Description = "Fresh fruits" };
            
            context.Products.AddRange(
                new Product { Name = "Apple", Price = 10, Quantity = 100, Category = category },
                new Product { Name = "Banana", Price = 20, Quantity = 50, Category = category }
            );
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var results = await service.GetProductsAsync(new ProductSearchCriteria());

            // Assert
            Assert.Equal(2, results.Items.Count);
        }

        [Fact]
        public async Task SearchProductsAsync_FiltersByName()
        {
            // Arrange
            using var context = CreateDbContext();
            var category = new Category { Name = "Fruits", Description = "Fresh fruits" };
            
            context.Products.AddRange(
                new Product { Name = "Apple", Price = 10, Quantity = 100, Category = category },
                new Product { Name = "Banana", Price = 20, Quantity = 50, Category = category }
            );
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var results = await service.GetProductsAsync(new ProductSearchCriteria { Keyword = "Apple" });

            // Assert
            Assert.Single(results.Items);
            Assert.Equal("Apple", results.Items[0].Name);
        }

        [Fact]
        public async Task SearchProductsAsync_FiltersByPrice()
        {
            // Arrange
            using var context = CreateDbContext();
            var category = new Category { Name = "Fruits", Description = "Fresh fruits" };
            
            context.Products.AddRange(
                new Product { Name = "Cheap", Price = 10, Category = category },
                new Product { Name = "Expensive", Price = 100, Category = category }
            );
            await context.SaveChangesAsync();

            var service = new ProductService(context);

            // Act
            var results = await service.GetProductsAsync(new ProductSearchCriteria { MinPrice = 50 });

            // Assert
            Assert.Single(results.Items);
            Assert.Equal("Expensive", results.Items[0].Name);
        }
    }
}
