using Project_MyShop_2025.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_MyShop_2025.Core.Services.Interfaces
{
    public interface IPromotionService
    {
        Task<List<Promotion>> GetAllPromotionsAsync();
        Task<List<Promotion>> GetActivePromotionsAsync();
        Task<Promotion?> GetPromotionByIdAsync(int id);
        Task<Promotion?> GetPromotionByCodeAsync(string code);
        Task<Promotion> CreatePromotionAsync(Promotion promotion);
        Task<Promotion> UpdatePromotionAsync(Promotion promotion);
        Task<bool> DeletePromotionAsync(int id);
        
        /// <summary>
        /// Validate a promotion code and return the promotion if valid
        /// </summary>
        Task<Promotion?> ValidateCodeAsync(string code, int orderSubtotal = 0);
        
        /// <summary>
        /// Calculate discount for a given order
        /// </summary>
        Task<int> CalculateDiscountAsync(string code, int orderSubtotal);
        
        /// <summary>
        /// Apply promotion to an order (increment usage count)
        /// </summary>
        Task<bool> ApplyPromotionAsync(int promotionId);
        
        /// <summary>
        /// Get promotions applicable to a specific product or category
        /// </summary>
        Task<List<Promotion>> GetApplicablePromotionsAsync(int? productId = null, int? categoryId = null);
    }
}
