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
        public string Description { get; set; }

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

        public string SupplierName { get; set; }

        [Required]
        public string Type { get; set; }


        public double Discount { get; set; }

        public string Colour { get; set; }

        public string Barcode { get; set; }
    }
}
    