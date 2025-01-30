using System.ComponentModel.DataAnnotations;

namespace WrapPOS.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal BuyPrice { get; set; }

        [Required]
        public decimal SellPrice { get; set; }

        [Required]
        public int Stock { get; set; }

        public string ImagePath { get; set; }

        [Required]
        public decimal Units { get; set; }

        [Required]
        public string UOM { get; set; }
    }
}
