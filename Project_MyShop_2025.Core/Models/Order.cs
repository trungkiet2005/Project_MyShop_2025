using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project_MyShop_2025.Core.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public double TotalPrice { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
