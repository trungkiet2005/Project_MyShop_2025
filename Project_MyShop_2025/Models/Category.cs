using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project_MyShop_2025.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
