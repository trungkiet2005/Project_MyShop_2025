using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_MyShop_2025.Core.Models
{
    /// <summary>
    /// Type of promotion/discount
    /// </summary>
    public enum PromotionType
    {
        /// <summary>Giảm theo phần trăm (e.g., 10%)</summary>
        Percentage = 0,
        
        /// <summary>Giảm số tiền cố định (e.g., 50,000đ)</summary>
        FixedAmount = 1,
        
        /// <summary>Mua X tặng Y (e.g., mua 2 tặng 1)</summary>
        BuyXGetY = 2
    }

    /// <summary>
    /// Represents a promotion or discount campaign
    /// </summary>
    public class Promotion
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Unique promotion code (e.g., "SUMMER2025", "NEWYEAR10")
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Display name for the promotion
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the promotion
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Type of discount (percentage, fixed amount, etc.)
        /// </summary>
        public PromotionType Type { get; set; } = PromotionType.Percentage;

        /// <summary>
        /// Discount value - meaning depends on Type:
        /// - Percentage: 10 means 10%
        /// - FixedAmount: 50000 means 50,000đ off
        /// - BuyXGetY: X value (buy X items)
        /// </summary>
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// For BuyXGetY: Y value (get Y free items)
        /// </summary>
        public int? FreeQuantity { get; set; }

        /// <summary>
        /// Minimum order amount to apply this promotion (VND)
        /// </summary>
        public int? MinOrderAmount { get; set; }

        /// <summary>
        /// Maximum discount amount (VND) - caps the discount for percentage type
        /// </summary>
        public int? MaxDiscountAmount { get; set; }

        /// <summary>
        /// Promotion start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Promotion end date
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Whether the promotion is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Maximum number of times this promotion can be used (null = unlimited)
        /// </summary>
        public int? UsageLimit { get; set; }

        /// <summary>
        /// Number of times this promotion has been used
        /// </summary>
        public int UsedCount { get; set; } = 0;

        /// <summary>
        /// If set, promotion only applies to this category
        /// </summary>
        public int? CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        /// <summary>
        /// If set, promotion only applies to this specific product
        /// </summary>
        public int? ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        /// <summary>
        /// Check if promotion is currently valid
        /// </summary>
        [NotMapped]
        public bool IsValid => IsActive && 
                               DateTime.Now >= StartDate && 
                               DateTime.Now <= EndDate &&
                               (!UsageLimit.HasValue || UsedCount < UsageLimit.Value);

        /// <summary>
        /// Calculate discount for a given order subtotal
        /// </summary>
        public int CalculateDiscount(int orderSubtotal)
        {
            if (!IsValid)
                return 0;

            if (MinOrderAmount.HasValue && orderSubtotal < MinOrderAmount.Value)
                return 0;

            int discount = Type switch
            {
                PromotionType.Percentage => (int)(orderSubtotal * DiscountValue / 100),
                PromotionType.FixedAmount => (int)DiscountValue,
                PromotionType.BuyXGetY => 0, // Handled differently in order creation
                _ => 0
            };

            // Apply max discount cap
            if (MaxDiscountAmount.HasValue && discount > MaxDiscountAmount.Value)
            {
                discount = MaxDiscountAmount.Value;
            }

            return discount;
        }
    }
}
