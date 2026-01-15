using Microsoft.EntityFrameworkCore;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Models;
using Project_MyShop_2025.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly ShopDbContext _context;

        public ProductService(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Product>> GetProductsAsync(ProductSearchCriteria criteria)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var keyword = criteria.Keyword.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(keyword) || 
                    (p.SKU != null && p.SKU.ToLower().Contains(keyword)) ||
                    (p.Description != null && p.Description.ToLower().Contains(keyword)));
            }

            if (criteria.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == criteria.CategoryId.Value);
            }

            if (criteria.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= criteria.MinPrice.Value);
            }

            if (criteria.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= criteria.MaxPrice.Value);
            }

            if (criteria.MinStock.HasValue)
            {
                query = query.Where(p => p.Quantity >= criteria.MinStock.Value);
            }

            if (criteria.MaxStock.HasValue)
            {
                query = query.Where(p => p.Quantity <= criteria.MaxStock.Value);
            }

            // Apply sorting
            query = criteria.SortBy switch
            {
                "NameDesc" => query.OrderByDescending(p => p.Name),
                "PriceAsc" => query.OrderBy(p => p.Price),
                "PriceDesc" => query.OrderByDescending(p => p.Price),
                "StockAsc" => query.OrderBy(p => p.Quantity),
                "StockDesc" => query.OrderByDescending(p => p.Quantity),
                _ => query.OrderBy(p => p.Name) // NameAsc default
            };

            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((criteria.Page - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                Page = criteria.Page,
                PageSize = criteria.PageSize
            };
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (product == null)
                return false;

            // Check if product is in any orders
            var isInOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
            if (isInOrders)
                return false; // Cannot delete product that has orders

            // Delete images first
            if (product.ProductImages.Any())
            {
                _context.ProductImages.RemoveRange(product.ProductImages);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProductImage>> GetProductImagesAsync(int productId)
        {
            return await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.DisplayOrder)
                .ToListAsync();
        }

        public async Task<ProductImage> AddProductImageAsync(ProductImage image)
        {
            _context.ProductImages.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<bool> DeleteProductImageAsync(int imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null)
                return false;

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 5, int count = 5)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Quantity > 0 && p.Quantity < threshold)
                .OrderBy(p => p.Quantity)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<(Product Product, int TotalSold)>> GetBestSellersAsync(int count = 5)
        {
            var productSales = await _context.OrderItems
                .Where(oi => oi.Order != null && oi.Order.Status == OrderStatus.Paid)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.TotalSold)
                .Take(count)
                .ToListAsync();

            var productIds = productSales.Select(ps => ps.ProductId).ToList();
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            return productSales
                .Select(ps => (
                    Product: products.FirstOrDefault(p => p.Id == ps.ProductId)!,
                    TotalSold: ps.TotalSold
                ))
                .Where(x => x.Product != null)
                .ToList();
        }

        public async Task<int> GetTotalProductCountAsync()
        {
            return await _context.Products.CountAsync();
        }
    }
}
