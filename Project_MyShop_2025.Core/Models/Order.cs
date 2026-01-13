using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        public int TotalPrice { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Created;

        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
