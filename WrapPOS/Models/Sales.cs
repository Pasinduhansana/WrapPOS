using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace WrapPOS.Models
{
    public class Sales
    {
        [Key]
        public int SalesId { get; set; }

        [Required]
        public DateTime SalesDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }  // Total selling price

        public decimal Discount { get; set; }

        [Required]
        public decimal NetAmount { get; set; }  // After discount

        public string CustomerName { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        [Required]
        public decimal Profit { get; set; }  // Profit calculation: TotalAmount - TotalCost

        // Navigation Property
        public virtual ObservableCollection<SalesItem> SalesItems { get; set; }
 
    }
}
