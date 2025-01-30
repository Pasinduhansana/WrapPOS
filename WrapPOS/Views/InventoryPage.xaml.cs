using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WrapPOS.Models;
using System.Collections.ObjectModel;
using WrapPOS.Data;
using Microsoft.Win32;

namespace WrapPOS.Views
{
    /// <summary>
    /// Interaction logic for InventoryPage.xaml
    /// </summary>
    public partial class InventoryPage : Page
    {
        //DatabaseService databaseservice = new DatabaseService();
        private static string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pos_database.db");
        Product product = new Product();
        public ObservableCollection<Product> Products { get; set; }
        private DatabaseService _databaseService;

        public InventoryPage()
        {
           
            InitializeComponent();
            _databaseService = new DatabaseService();
            Products = new ObservableCollection<Product>();
            LoadData();

        }

        private void LoadData()
        {
            // Get all products from the database
            var productsFromDb = _databaseService.GetProducts();

            // Clear the ObservableCollection and add products
            Products.Clear();
            foreach (var product in productsFromDb)
            {
                Products.Add(product);
            }

            // Set the ItemsSource for the ListView (already bound in XAML)
            InventoryListView.ItemsSource = Products;
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            var newProduct = new Product
            {
                Name = "New Product",
                BuyPrice = 0.00m,
                SellPrice = 0.00m,
                Stock = 0,
                Units = 0.00m, // Matching with the Product class
                UOM = "Unit",  // Default value for Unit of Measure
                ImagePath = "default.png"
            };

            try
            {
                MessageBox.Show(dbPath);
                using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        INSERT INTO Products (Name, BuyPrice, SellPrice, Stock, ImagePath, Units, UOM)
                        VALUES ($name, $buyPrice, $sellPrice, $stock, $imagePath, $units, $uom);
                    ";
                    command.Parameters.AddWithValue("$name", newProduct.Name);
                    command.Parameters.AddWithValue("$buyPrice", newProduct.BuyPrice);
                    command.Parameters.AddWithValue("$sellPrice", newProduct.SellPrice);
                    command.Parameters.AddWithValue("$stock", newProduct.Stock);
                    command.Parameters.AddWithValue("$imagePath", (object)newProduct.ImagePath ?? DBNull.Value);
                    command.Parameters.AddWithValue("$units", newProduct.Units);
                    command.Parameters.AddWithValue("$uom", newProduct.UOM);
                    command.ExecuteNonQuery();
                }
                //databaseservice.AddProduct(newProduct);  // Call the previously created AddProduct function
                System.Windows.MessageBox.Show("New product added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void UpdateItem_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryListView.SelectedItem is Product selectedProduct)
            {
                selectedProduct.Name = "Updated Product";
                selectedProduct.BuyPrice += 5.00m;
                selectedProduct.SellPrice += 10.00m;
                selectedProduct.Stock += 10;
                selectedProduct.Units += 50.00m;
                selectedProduct.UOM = "Updated UOM";

                try
                {
                   // databaseservice.UpdateProduct(selectedProduct);  // Call the service function

                    MessageBox.Show("Product updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a product to update.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (InventoryListView.SelectedItem is Product selectedProduct)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {selectedProduct.Name}?",
                                             "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _databaseService.DeleteProduct(selectedProduct.ProductId);

                        // Optionally, remove the product from the ListView after deletion
                        var productToRemove = InventoryListView.SelectedItem as Product;
                        if (productToRemove != null)
                        {
                            var productList = (ObservableCollection<Product>)InventoryListView.ItemsSource;
                            productList.Remove(productToRemove);
                        }

                        MessageBox.Show("Product deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a product to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private void ExportInventory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export Inventory Clicked");
            // Implement export logic to CSV or Excel
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {

            string name = ProductNameTextBox.Text;
            string buyPriceText = BuyPriceTextBox.Text;
            string sellPriceText = SellPriceTextBox.Text;
            string stockText = StockTextBox.Text;
            string unitsText = UnitsTextBox.Text;
            var uom = UOMComboBox.SelectedItem as ComboBoxItem;

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(buyPriceText) ||
                string.IsNullOrWhiteSpace(sellPriceText) ||
                string.IsNullOrWhiteSpace(stockText) ||
                string.IsNullOrWhiteSpace(unitsText) ||
                uom == null)
            {
                MessageBox.Show("Please fill all fields before submitting.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(buyPriceText, out decimal buyPrice) ||
                !decimal.TryParse(sellPriceText, out decimal sellPrice) ||
                !int.TryParse(stockText, out int stock) ||
                !decimal.TryParse(unitsText, out decimal units))
            {
                MessageBox.Show("Please enter valid numbers for Price, Stock, and Weight.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Add the product to the database or ObservableCollection
            var newProduct = new Product
            {
                Name = name,
                BuyPrice = buyPrice,
                SellPrice = sellPrice,
                Stock = stock,
                Units = units,
                UOM = uom.Content.ToString() // Get the selected UOM value
            };

            // Call your database service to save the new product
            var databaseService = new DatabaseService();
            databaseService.AddProduct(newProduct);

            MessageBox.Show("Product added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Optionally clear the fields after adding the product
            ProductNameTextBox.Clear();
            BuyPriceTextBox.Clear();
            SellPriceTextBox.Clear();
            StockTextBox.Clear();
            UnitsTextBox.Clear();
            UOMComboBox.SelectedIndex = -1;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "All files (*.*)|*.*|Text files (*.txt)|*.txt|PDF files (*.pdf)|*.pdf|Excel files (*.xls;*.xlsx;*.xlsm)|*.xls;*.xlsx;*.xlsm";

            if (openFileDialog.ShowDialog() == true)
            {
                string[] selectedFiles = openFileDialog.SafeFileNames;
                string[] sourceFiles = openFileDialog.FileNames;

                string destinationFolder = AppDomain.CurrentDomain.BaseDirectory + "\\Attachments\\" + product.ProductId;

                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }


                foreach (string sourcefile in sourceFiles)
                {
                    string selectedFile = System.IO.Path.GetFileName(sourcefile);
                    string destinationFile = System.IO.Path.Combine(destinationFolder, selectedFile);

                    // Check if the file already exists
                    if (File.Exists(destinationFile))
                    {
                        var result = MessageBox.Show($"The file '{selectedFile}' already exists. Do you want to overwrite it?", "File exists", MessageBoxButton.YesNoCancel);
                        if (result == MessageBoxResult.Yes)
                        {
                            File.Copy(sourcefile, destinationFile, true);
                        }
                    }
                    else
                    {
                        File.Copy(sourcefile, destinationFile);

                    }
                }
            }
        }

        private void Drop_rectangle_Drop(object sender, DragEventArgs e)
        {

        }
    }
}
