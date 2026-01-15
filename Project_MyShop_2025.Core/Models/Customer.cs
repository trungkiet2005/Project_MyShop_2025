using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project_MyShop_2025.Core.Models
{
    /// <summary>
    /// Represents a customer in the system
    /// </summary>
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Customer creation date
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Loyalty points accumulated from purchases
        /// </summary>
        public int LoyaltyPoints { get; set; } = 0;

        /// <summary>
        /// Notes about the customer
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Whether the customer is active (soft delete)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Navigation property for customer orders
        /// </summary>
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
