using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace WrapPOS.Models
{
    public class Product : INotifyPropertyChanged
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

        public string FullImagePath
        {
            get
            {
                // Get absolute path to the bin folder
                string basePath = AppDomain.CurrentDomain.BaseDirectory;

                // Combine base path with the stored relative path
                if(ImagePath is null)
                {
                    return ImagePath;
                }
                else
                {
                    if (basePath.Contains("\\"))
                    {
                        basePath=basePath.Replace('\\', '/');
                    }
                    return Path.Combine(basePath, ImagePath);
                }

            }
        }

        private int _quantity = 1;

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity)); // Notify UI
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
    