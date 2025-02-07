using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WrapPOS.Models
{
    public class SalesItem
    {
        [Key]
        public int SalesItemId { get; set; }

        [Required]
        public int SalesId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }  // Selling price per unit

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal TotalPrice { get; set; } // Quantity * Selling Price

        [Required]
        public decimal BuyPrice { get; set; }  // Buy price per unit (for profit calculation)

        public decimal TotalCost => BuyPrice * Quantity;  // Total cost for this item

        public string UOM { get; set; }

        public decimal Discount { get; set; }

        public string Colour { get; set; }

        public string Barcode { get; set; }

        public string ImagePath { get; set; }

        // Navigation Property
        [ForeignKey("SalesId")]
        public virtual Sales Sales { get; set; }
    }
}
