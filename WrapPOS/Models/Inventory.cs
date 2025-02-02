using System;
using System.ComponentModel.DataAnnotations;

namespace WrapPOS.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        // Navigation property to Product
        public virtual Product Product { get; set; }
    }
}
