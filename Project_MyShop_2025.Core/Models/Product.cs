using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_MyShop_2025.Core.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? SKU { get; set; }

        public string? Description { get; set; }

        [Range(0, int.MaxValue)]
        public int Price { get; set; }

        public int ImportPrice { get; set; }

        public string? Image { get; set; }

        public int Quantity { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        // Navigation property for multiple images
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}
