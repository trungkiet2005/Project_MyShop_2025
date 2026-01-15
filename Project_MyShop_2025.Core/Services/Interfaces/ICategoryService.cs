using Project_MyShop_2025.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);
        Task<int> GetProductCountByCategoryAsync(int categoryId);
    }
}
