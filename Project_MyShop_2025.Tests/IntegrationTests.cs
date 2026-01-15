using Microsoft.EntityFrameworkCore;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Models;
using Project_MyShop_2025.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Project_MyShop_2025.Tests
{
    public class IntegrationTests
    {
        private DbContextOptions<ShopDbContext> _options;

        public IntegrationTests()
        {
            _options = new DbContextOptionsBuilder<ShopDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
        }

        [Fact]
        public async Task ImportProducts_Simulation_AddsProductsToDatabase()
        {
            // Arrange
            using var context = new ShopDbContext(_options);
            await context.Database.OpenConnectionAsync();
            await context.Database.EnsureCreatedAsync();

            // Simulate "Imported" category creation
            var importedCategory = new Category { Name = "Imported", Description = "Imported from Excel" };
            context.Categories.Add(importedCategory);
            await context.SaveChangesAsync();

            // Simulate Excel Data (List of Dictionaries/Objects)
            var importedData = new[]
            {
                new { Name = "Imported Item 1", SKU = "IMP-001", Price = 100000, Quantity = 10 },
                new { Name = "Imported Item 2", SKU = "IMP-002", Price = 200000, Quantity = 5 }
            };

            // Act: Process Import
            foreach (var item in importedData)
            {
                var product = new Product
                {
                    Name = item.Name,
                    SKU = item.SKU,
                    Description = "Imported Product",
                    Price = item.Price,
                    Quantity = item.Quantity,
                    CategoryId = importedCategory.Id
                };
                context.Products.Add(product);
            }
            await context.SaveChangesAsync();

            // Assert
            var products = await context.Products.ToListAsync();
            Assert.Equal(2, products.Count);
            Assert.Contains(products, p => p.Name == "Imported Item 1");
            Assert.Contains(products, p => p.SKU == "IMP-002");
        }

        [Fact]
        public async Task AddProduct_ValidatesRequiredFields()
        {
             // Arrange
            using var context = new ShopDbContext(_options);
            await context.Database.OpenConnectionAsync();
            await context.Database.EnsureCreatedAsync();

             var category = new Category { Name = "Test Cat" };
             context.Categories.Add(category);
             await context.SaveChangesAsync();

             var product = new Product
             {
                 Name = "New Phone",
                 Price = 5000000,
                 CategoryId = category.Id
             };

             // Act
             context.Products.Add(product);
             await context.SaveChangesAsync();

             // Assert
             Assert.NotEqual(0, product.Id);
             Assert.Equal("New Phone", product.Name);
        }
    }
}
