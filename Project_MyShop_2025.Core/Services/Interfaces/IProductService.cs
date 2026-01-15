using Project_MyShop_2025.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Interfaces
{
    public class ProductSearchCriteria
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
        public string SortBy { get; set; } = "NameAsc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public interface IProductService
    {
        Task<PagedResult<Product>> GetProductsAsync(ProductSearchCriteria criteria);
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        
        // Product Images
        Task<List<ProductImage>> GetProductImagesAsync(int productId);
        Task<ProductImage> AddProductImageAsync(ProductImage image);
        Task<bool> DeleteProductImageAsync(int imageId);
        
        // Dashboard queries
        Task<List<Product>> GetLowStockProductsAsync(int threshold = 5, int count = 5);
        Task<List<(Product Product, int TotalSold)>> GetBestSellersAsync(int count = 5);
        Task<int> GetTotalProductCountAsync();
    }
}
