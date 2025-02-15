using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace WrapPOS.Models
{
    public class Inventory : INotifyPropertyChanged
    {
        private int _inventoryId;
        private int _productId;
        private int _quantity;
        private DateTime _purchaseDate;
        private DateTime? _expiryDate;
        private Product _product;

        [Key]
        public int InventoryId
        {
            get => _inventoryId;
            set
            {
                if (_inventoryId != value)
                {
                    _inventoryId = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required]
        public int ProductId
        {
            get => _productId;
            set
            {
                if (_productId != value)
                {
                    _productId = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required]
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required]
        public DateTime PurchaseDate
        {
            get => _purchaseDate;
            set
            {
                if (_purchaseDate != value)
                {
                    _purchaseDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? ExpiryDate
        {
            get => _expiryDate;
            set
            {
                if (_expiryDate != value)
                {
                    _expiryDate = value;
                    OnPropertyChanged();
                }
            }
        }

        // Navigation property to Product
        public virtual Product Product
        {
            get => _product;
            set
            {
                if (_product != value)
                {
                    _product = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}