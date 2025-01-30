using System;
using System.ComponentModel.DataAnnotations;

namespace WrapPOS.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public DateTime PurchaseDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string SupplierName { get; set; }

        public string WarehouseLocation { get; set; }

        // Navigation property to Product
        public virtual Product Product { get; set; }
    }
}
