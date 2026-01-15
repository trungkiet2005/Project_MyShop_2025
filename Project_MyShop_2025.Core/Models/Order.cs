using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_MyShop_2025.Core.Models
{
    public enum OrderStatus
    {
        Created = 0,    // Mới tạo
        Paid = 1,       // Đã thanh toán
        Cancelled = 2   // Đã hủy
    }

    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Subtotal before discount (VND)
        /// </summary>
        public int SubTotal { get; set; }

        /// <summary>
        /// Discount amount applied (VND)
        /// </summary>
        public int DiscountAmount { get; set; } = 0;

        /// <summary>
        /// Final total after discount: TotalPrice = SubTotal - DiscountAmount
        /// </summary>
        public int TotalPrice { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Created;

        // Customer info - kept for backward compatibility and non-registered customers
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }

        // Reference to registered Customer (optional)
        public int? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer? Customer { get; set; }

        // Promotion applied to this order
        public int? PromotionId { get; set; }

        [ForeignKey(nameof(PromotionId))]
        public Promotion? Promotion { get; set; }

        /// <summary>
        /// Promotion code used (stored for reference even if promotion is deleted)
        /// </summary>
        [MaxLength(50)]
        public string? PromotionCode { get; set; }

        /// <summary>
        /// Notes for the order
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Calculate and set totals based on items and promotion
        /// </summary>
        public void CalculateTotals()
        {
            SubTotal = 0;
            foreach (var item in Items)
            {
                item.TotalPrice = item.Price * item.Quantity;
                SubTotal += item.TotalPrice;
            }

            // Apply promotion discount
            if (Promotion != null && Promotion.IsValid)
            {
                DiscountAmount = Promotion.CalculateDiscount(SubTotal);
                PromotionCode = Promotion.Code;
            }
            else
            {
                DiscountAmount = 0;
            }

            TotalPrice = SubTotal - DiscountAmount;
            if (TotalPrice < 0) TotalPrice = 0;
        }
    }
}
