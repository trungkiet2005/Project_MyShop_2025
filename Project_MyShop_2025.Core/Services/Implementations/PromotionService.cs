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
    public class PromotionService : IPromotionService
    {
        private readonly ShopDbContext _context;

        public PromotionService(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<List<Promotion>> GetAllPromotionsAsync()
        {
            return await _context.Promotions
                .Include(p => p.Category)
                .Include(p => p.Product)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<List<Promotion>> GetActivePromotionsAsync()
        {
            var now = DateTime.Now;
            return await _context.Promotions
                .Include(p => p.Category)
                .Include(p => p.Product)
                .Where(p => p.IsActive && 
                            p.StartDate <= now && 
                            p.EndDate >= now &&
                            (!p.UsageLimit.HasValue || p.UsedCount < p.UsageLimit.Value))
                .OrderByDescending(p => p.DiscountValue)
                .ToListAsync();
        }

        public async Task<Promotion?> GetPromotionByIdAsync(int id)
        {
            return await _context.Promotions
                .Include(p => p.Category)
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Promotion?> GetPromotionByCodeAsync(string code)
        {
            return await _context.Promotions
                .Include(p => p.Category)
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.Code.ToUpper() == code.ToUpper());
        }

        public async Task<Promotion> CreatePromotionAsync(Promotion promotion)
        {
            promotion.Code = promotion.Code.ToUpper();
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<Promotion> UpdatePromotionAsync(Promotion promotion)
        {
            promotion.Code = promotion.Code.ToUpper();
            _context.Promotions.Update(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<bool> DeletePromotionAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
                return false;

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Promotion?> ValidateCodeAsync(string code, int orderSubtotal = 0)
        {
            var promotion = await GetPromotionByCodeAsync(code);
            if (promotion == null)
                return null;

            // Check if promotion is valid
            if (!promotion.IsValid)
                return null;

            // Check minimum order amount
            if (promotion.MinOrderAmount.HasValue && orderSubtotal < promotion.MinOrderAmount.Value)
                return null;

            return promotion;
        }

        public async Task<int> CalculateDiscountAsync(string code, int orderSubtotal)
        {
            var promotion = await ValidateCodeAsync(code, orderSubtotal);
            if (promotion == null)
                return 0;

            return promotion.CalculateDiscount(orderSubtotal);
        }

        public async Task<bool> ApplyPromotionAsync(int promotionId)
        {
            var promotion = await _context.Promotions.FindAsync(promotionId);
            if (promotion == null)
                return false;

            promotion.UsedCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Promotion>> GetApplicablePromotionsAsync(int? productId = null, int? categoryId = null)
        {
            var now = DateTime.Now;
            var query = _context.Promotions
                .Include(p => p.Category)
                .Include(p => p.Product)
                .Where(p => p.IsActive && 
                            p.StartDate <= now && 
                            p.EndDate >= now &&
                            (!p.UsageLimit.HasValue || p.UsedCount < p.UsageLimit.Value));

            if (productId.HasValue)
            {
                // Get the product's category
                var product = await _context.Products.FindAsync(productId.Value);
                if (product != null)
                {
                    query = query.Where(p => 
                        p.ProductId == null && p.CategoryId == null || // General promotions
                        p.ProductId == productId.Value || // Product specific
                        p.CategoryId == product.CategoryId); // Category specific
                }
            }
            else if (categoryId.HasValue)
            {
                query = query.Where(p => 
                    p.ProductId == null && p.CategoryId == null || // General promotions
                    p.CategoryId == categoryId.Value); // Category specific
            }
            else
            {
                // Only general promotions (not specific to product or category)
                query = query.Where(p => p.ProductId == null && p.CategoryId == null);
            }

            return await query.OrderByDescending(p => p.DiscountValue).ToListAsync();
        }
    }
}
