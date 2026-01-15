using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_MyShop_2025.Core.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        public int Price { get; set; } // Unit sale price -
        
        public int TotalPrice { get; set; }

        [NotMapped]
        public string PriceFormatted => $"₫{Price:N0}";

        [NotMapped]
        public string TotalPriceFormatted => $"₫{TotalPrice:N0}";
    }
}
