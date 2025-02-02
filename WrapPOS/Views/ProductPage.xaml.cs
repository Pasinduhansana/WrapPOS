using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WrapPOS.Data;
using WrapPOS.Models;
using Path = System.IO.Path;
using ClosedXML.Excel;


namespace WrapPOS.Views
{
    /// <summary>
    /// Interaction logic for ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        private static string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pos_database.db");
        Product product = new Product();
        public ObservableCollection<Product> Products { get; set; }
        private CollectionViewSource CollectionView;

        private DatabaseService _databaseService;
        private string imagePath;
        private BackgroundWorker _backgroundWorker;

        public ProductPage()
        {
            _backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true, 
                WorkerSupportsCancellation = true 
            };
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;


            InitializeComponent();
            _databaseService = new DatabaseService();
            Products = new ObservableCollection<Product>();
            _backgroundWorker.RunWorkerAsync();

            CollectionView = new CollectionViewSource { Source = Products };
            CollectionView.View.Filter = FilterProducts; // Set filter logic
            ProductListView.ItemsSource = CollectionView.View;
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
          
        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            // Get all products from the database
            var productsFromDb = _databaseService.GetProducts();

            // Clear the ObservableCollection and add products
            Application.Current.Dispatcher.Invoke(() =>
            {
                Products.Clear();
                foreach (var product in productsFromDb)
                {
                    Products.Add(product);
                }

               // ProductListView.ItemsSource = Products;
            });
        }

        private string GetSelectedUnitShortForm(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Tag.ToString().Split(' ')[1].Trim('(', ')');
            }
            return string.Empty;
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Retrieve values from the .xaml defined elements
                string name = ProductNameTextBox.Text;
                string description = (bool)Description_checkbox.IsChecked ? ProductDescriptionTextBox.Text + " - " + UnitsTextBox.Text + UOMComboBox.Text:ProductDescriptionTextBox.Text;
                decimal buyPrice = decimal.Parse(BuyPriceTextBox.Text);
                decimal sellPrice = decimal.Parse(SellPriceTextBox.Text);
                string uom = GetSelectedUnitShortForm(UOMComboBox);
                string Units = UnitsTextBox.Text;
                string supplierName = Supplier_textbox.Text;
                string type = TypeComboBox.Text;
                double discount = double.Parse(Discount_textbox.Text);
                string colour = Colour_textbox.Text;
                string barcode = Barcode_textbox.Text;



                // Create a new Product object
                Product newProduct = new Product
                {
                    Name = name,
                    Description = description,
                    BuyPrice = buyPrice,
                    SellPrice = sellPrice,
                    Stock = 1,
                    Units = decimal.Parse(Units),
                    UOM = uom,
                    SupplierName = supplierName,
                    Type = type,
                    Discount = discount,
                    Colour = colour,
                    Barcode = barcode
                };

                // Call the AddProduct method from DatabaseServices
                _databaseService.AddProduct(newProduct);

                if (!string.IsNullOrEmpty(imagePath))
                {
                    string newImagePath = SaveImageToFolder(imagePath, newProduct.ProductId);
                    newProduct.ImagePath = newImagePath;

                    // Update the product in the database with the new ImagePath
                    _databaseService.UpdateProduct(newProduct);
                }

                // Show success message
                MessageBox.Show("Product added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear the form fields (optional)
                ClearFormFields();
                LoadData();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter valid numeric values for Buy Price, Sell Price, Stock, Units, and Discount.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Method to clear form fields (optional)
        private void ClearFormFields()
        {
            // Clear all input fields
            ProductNameTextBox.Clear();
            ProductDescriptionTextBox.Clear();
            BuyPriceTextBox.Clear();
            SellPriceTextBox.Clear();
            UnitsTextBox.Clear();
            Supplier_textbox.Clear();
            Colour_textbox.Clear();
            Barcode_textbox.Clear();
            Discount_textbox.Clear();
            UOMComboBox.SelectedIndex = -1;
            TypeComboBox.SelectedIndex = -1;
            imagePath = string.Empty;
            Product_image.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/default.jpg"));


        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button deleteButton && deleteButton.DataContext is Product selectedProduct)
            {
                // Confirmation message
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete '{selectedProduct.Name}'?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    // Delete product from database
                    var databaseService = new DatabaseService();
                    databaseService.DeleteProduct(selectedProduct.ProductId);

                    // Remove the product from the UI list
                    Products.Remove(selectedProduct);
                }
            }
        }

        private Product selectedProduct;
        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button editButton && editButton.DataContext is Product product)
            {
                selectedProduct = product; // Store for updating

                // Populate fields with product data
                ProductNameTextBox.Text = product.Name;
                ProductDescriptionTextBox.Text = product.Description;
                BuyPriceTextBox.Text = product.BuyPrice.ToString();
                SellPriceTextBox.Text = product.SellPrice.ToString();
                UOMComboBox.SelectedValue = product.UOM;
                UnitsTextBox.Text = product.Units.ToString();
                TypeComboBox.SelectedValue = product.Type;
                Supplier_textbox.Text = product.SupplierName;
                Colour_textbox.Text = product.Colour;
                Discount_textbox.Text = product.Discount.ToString();
                Barcode_textbox.Text = product.Barcode;
                // Check if ImagePath is not null, empty, or invalid
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    string productImage = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, product.ImagePath);

                    if (File.Exists(productImage))
                    {
                        Product_image.ImageSource = new BitmapImage(new Uri(productImage, UriKind.Absolute));
                    }
                    else
                    {
                        // Set a default resource image if file not found
                        Product_image.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/default.jpg"));
                    }
                }
                else
                {
                    // Set a default resource image if path is null
                    Product_image.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/default.jpg"));
                }



                // Change button text to "Update Product"

                AddProductButton.Content = "Update Product";
                AddProductButton.Click -= AddProduct_Click; // Remove existing event
                AddProductButton.Click += UpdateProduct_Click; // Attach update event
                Cancel_Update_button.Visibility = Visibility.Visible;
            }

        }

        private void UpdateProduct_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProduct != null)
            {
                // Update product details
                selectedProduct.Name = ProductNameTextBox.Text;
                selectedProduct.Description = (bool)Description_checkbox.IsChecked ? ProductDescriptionTextBox.Text + " - " + UnitsTextBox.Text + UOMComboBox.Text : ProductDescriptionTextBox.Text;
                selectedProduct.BuyPrice = decimal.Parse(BuyPriceTextBox.Text);
                selectedProduct.SellPrice = decimal.Parse(SellPriceTextBox.Text);
                selectedProduct.UOM = GetSelectedUnitShortForm(UOMComboBox);
                selectedProduct.Units = int.Parse(UnitsTextBox.Text);
                selectedProduct.Type = TypeComboBox.Text?.ToString();
                selectedProduct.SupplierName = Supplier_textbox.Text;
                selectedProduct.Colour = Colour_textbox.Text;
                selectedProduct.Discount = double.Parse(Discount_textbox.Text);
                selectedProduct.Barcode = Barcode_textbox.Text;

                if (!string.IsNullOrEmpty(imagePath))
                {
                    string newImagePath = SaveImageToFolder(imagePath, selectedProduct.ProductId);
                    selectedProduct.ImagePath = newImagePath;

                }

                // Save to database
                var databaseService = new DatabaseService();
                databaseService.UpdateProduct(selectedProduct);

                // Reset button back to "Add Product"
                AddProductButton.Content = "Add Product";
                AddProductButton.Click -= UpdateProduct_Click;
                AddProductButton.Click += AddProduct_Click;
                Cancel_Update_button.Visibility = Visibility.Collapsed;
                _backgroundWorker.RunWorkerAsync();
                selectedProduct = null; // Clear selection
                ClearFormFields();
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export Clicked");
        }

        private void Product_image_change(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Image files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tiff";
                dialog.Title = "Please select file to upload as product image";
                dialog.RestoreDirectory = true;

                List<string> Imported_Column_List = new List<string>();

                if (dialog.ShowDialog() == true)
                {
                    Product_image.ImageSource = new BitmapImage(new Uri(dialog.FileName));
                    imagePath = dialog.FileName;

                }

            }
            catch (Exception e1)
            {

                MessageBox.Show(e1.Message + ". Please Try again !", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            }
        }

        private string SaveImageToFolder(string sourceImagePath, int productId)
        {
            try
            {
                // Define the target folder path
                string targetFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", productId.ToString());

                // Create the folder if it doesn't exist
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                // Define the target file path
                string fileName = Path.GetFileName(sourceImagePath);
                string targetFilePath = Path.Combine(targetFolder, fileName);

                // Copy the image to the target folder
                File.Copy(sourceImagePath, targetFilePath, overwrite: true);

                // Return the relative path to the image
                return Path.Combine("Images", productId.ToString(), fileName);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save image: " + ex.Message);
            }
        }
        private void Cancel_Update_button_Click(object sender, RoutedEventArgs e)
        {
            ClearFormFields();

            // Switch button text to "Add Product"
            AddProductButton.Content = "Add Product";
            AddProductButton.Click -= UpdateProduct_Click;
            AddProductButton.Click += AddProduct_Click;
            Cancel_Update_button.Visibility = Visibility.Collapsed;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionView.View.Refresh();
        }

        private bool FilterProducts(object item)
        {
            if (item is Product product)
            {
                string filterText = SearchBox.Text.ToLower();
                return string.IsNullOrEmpty(filterText) ||
                       product.Name.ToLower().Contains(filterText) ||
                       product.Description.ToLower().Contains(filterText) ||
                       product.Barcode.ToLower().Contains(filterText) ||
                       product.SupplierName.ToLower().Contains(filterText) ||
                       product.Colour.ToLower().Contains(filterText) ||
                       product.Type.ToLower().Contains(filterText) ||
                       product.ProductId.ToString().ToLower().Contains(filterText);

            }
            return false;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var products = Products; // Replace with the actual list of products

            // Create a new Excel package
            using (var workbook = new XLWorkbook())
            {
                // Add a worksheet to the package
                var worksheet = workbook.Worksheets.Add("Products");

                // Add header row
                worksheet.Cell(1, 1).Value = "Product ID";
                worksheet.Cell(1, 2).Value = "Product Name";
                worksheet.Cell(1, 3).Value = "Product Description";
                worksheet.Cell(1, 4).Value = "Buy Price";
                worksheet.Cell(1, 5).Value = "Sell Price";
                worksheet.Cell(1, 6).Value = "Units";
                worksheet.Cell(1, 7).Value = "UOM";
                worksheet.Cell(1, 8).Value = "Supplier Name";
                worksheet.Cell(1, 9).Value = "Type";
                worksheet.Cell(1, 10).Value = "Discount";
                worksheet.Cell(1, 11).Value = "Colour";
                worksheet.Cell(1, 12).Value = "Barcode";

                // Add product data to the sheet
                int row = 2;
                foreach (var product in products)
                {
                    worksheet.Cell(row, 1).Value = product.ProductId;
                    worksheet.Cell(row, 2).Value = product.Name;
                    worksheet.Cell(row, 3).Value = product.Description;
                    worksheet.Cell(row, 4).Value = product.BuyPrice;
                    worksheet.Cell(row, 5).Value = product.SellPrice;
                    worksheet.Cell(row, 6).Value = product.Units;
                    worksheet.Cell(row, 7).Value = product.UOM;
                    worksheet.Cell(row, 8).Value = product.SupplierName;
                    worksheet.Cell(row, 9).Value = product.Type;
                    worksheet.Cell(row, 10).Value = product.Discount;
                    worksheet.Cell(row, 11).Value = product.Colour;
                    worksheet.Cell(row, 12).Value = product.Barcode;
                    row++;
                }

                // Save the Excel file
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = "Products.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var filePath = saveFileDialog.FileName;
                    workbook.SaveAs(filePath);
                    MessageBox.Show("Products exported successfully!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
