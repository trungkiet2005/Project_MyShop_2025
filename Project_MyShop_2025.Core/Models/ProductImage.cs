using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_MyShop_2025.Core.Models
{
    public class ProductImage
    {


        [Key]
        public int Id { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }
    }
}
