using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WrapPOS.Models;
using System.Collections.ObjectModel;
using WrapPOS.Data;
using Microsoft.Win32;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors.Layout;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Path = System.IO.Path;
using WrapPOS.Views;
using OfficeOpenXml;
using ClosedXML.Excel;
using MaterialDesignThemes.Wpf;

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
        Product selectedProduct = new Product();
        Inventory inventory = new Inventory();
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<Product> filteredProducts { get; set; }
        public ObservableCollection<Inventory> Inventories { get; set; }
        private DatabaseService _databaseService;
        private BackgroundWorker _backgroundWorker;
        private CollectionViewSource CollectionView;
        public SnackbarMessageQueue SnackbarMessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));

        public InventoryPage()
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
            Inventories = new ObservableCollection<Inventory>();
            _backgroundWorker.RunWorkerAsync();

            CollectionView = new CollectionViewSource { Source = Inventories };
            CollectionView.View.Filter = FilterInventory; // Set filter logic
            InventoryListView.ItemsSource = CollectionView.View;

        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Load_Func();
        }

        private void Load_Func()
        {
            var pinventoryFromDb = _databaseService.GetInventoryItems();
            var productsFromDb = _databaseService.GetProducts();

            Application.Current.Dispatcher.Invoke(() =>
                {
                    Inventories.Clear();
                    foreach (var item in pinventoryFromDb)
                    {
                        Inventories.Add(item);
                    }

                    Products.Clear();
                    foreach (var product in productsFromDb)
                    {
                        Products.Add(product);
                    }

                    SuggestionsListBox.ItemsSource = Products.Select(p => p.Name).ToList();
                
                });

        }


        private void ProductComboBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var searchText = ProductTextBox.Text.ToLower();

            // Filter the products based on the entered text
            filteredProducts = new ObservableCollection<Product>(
                Products.Where(p => p.Name.ToLower().Contains(searchText) ||
                                    p.Description.Contains(searchText) ||
                                    p.ProductId.ToString().ToLower().Contains(searchText))
            );

            // Bind the filtered products to the ListBox
            SuggestionsListBox.ItemsSource = filteredProducts.Select(p => p.Name).ToList();

            // Show the ListBox if there are filtered items
            SuggestionsListBox.Visibility = filteredProducts.Select(p => p.Name).ToList().Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuggestionsListBox.SelectedItem is string selectedProductName)
            {
                selectedProduct = Products.FirstOrDefault(p => p.Name == selectedProductName);
                
                if (selectedProduct != null)
                {
                    ProductTextBox.Text = selectedProduct.Name;
                    BarcodeTextBox.Text = selectedProduct.Barcode;

                    ProductTextBox.Text = selectedProduct.Name;

                    // Update product details inside the card
                    ProductNameTextBlock.Text = selectedProduct.Name;
                    ProductDescriptionTextBlock.Text = selectedProduct.Description;
                    ProductBarcodeTextBlock.Text = "Barcode: " + selectedProduct.Barcode;
                    ProductPriceTextBlock.Text = "Price: LKR " + selectedProduct.SellPrice.ToString("F2");

                    // Set the Product Image (assuming the image URL/path is stored in the Product object)
                    if (!string.IsNullOrEmpty(selectedProduct.ImagePath))
                    {
                        string productImage = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, selectedProduct.ImagePath);

                        if (File.Exists(productImage))
                        {
                            ProductImage.ImageSource = new BitmapImage(new Uri(productImage, UriKind.Absolute));
                        }
                        else
                        {
                            // Set a default resource image if file not found
                            ProductImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/default.jpg"));
                        }
                    }
                    else
                    {
                        // Set a default resource image if path is null
                        ProductImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/default.jpg"));
                    }


                }
                SuggestionsListBox.Visibility = Visibility.Collapsed; // Hide the ListBox after selection
            }
        }

        private void ScanProduct(object sender, RoutedEventArgs e)
        {
            try
            {
                string scannedBarcode = BarcodeTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(scannedBarcode))
                {
                    Product foundProduct = Products.FirstOrDefault(p => p.Barcode == scannedBarcode);

                    if (foundProduct != null)
                    {
                        SuggestionsListBox.SelectedItem = foundProduct.Name;
                    }
                    else
                    {
                        SnackbarMessageQueue.Enqueue("Product not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue("Error scanning product: " + ex.Message);
            }
        }

        private void AddInventory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SuggestionsListBox.SelectedItem is string selectedProductName && int.TryParse(QuantityTextBox.Text, out int quantity))
                {

                    Inventory newInventory = new Inventory
                    {
                        ProductId = selectedProduct.ProductId,
                        Quantity = quantity,
                        PurchaseDate = PurchaseDatePicker.SelectedDate ?? DateTime.Now,
                        ExpiryDate = ExpiryDatePicker.SelectedDate
                    };

                    _databaseService.AddInventory(newInventory);
                    
                    _backgroundWorker.RunWorkerAsync();
                    SnackbarMessageQueue.Enqueue("Inventory added successfully.");
                }
                else
                {
                    SnackbarMessageQueue.Enqueue("Please select a product and enter a valid quantity.");
                }
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue("Error adding inventory: " + ex.Message);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var inventoryId = (int)button.Tag;

            // Your edit logic here (e.g., open an edit dialog, etc.)
            SnackbarMessageQueue.Enqueue($"Edit Inventory with ID: {inventoryId}");
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var inventoryId = (int)button.Tag;

            _databaseService.DeleteInventory(inventoryId);
            // Your delete logic here (e.g., delete from database, etc.)
            SnackbarMessageQueue.Enqueue($"Delete Inventory with ID: {inventoryId}");
        }


        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionView.View.Refresh();
        }
        private bool FilterInventory(object item)
        {
            if (item is Inventory inventory)
            {
                string filterText = SearchBox.Text.ToLower();
                return string.IsNullOrEmpty(filterText) ||
                       inventory.Product.Description.Contains(filterText) ||
                       inventory.Product.Name.ToLower().Contains(filterText) ||
                       inventory.Product.Barcode.ToLower().Contains(filterText) ||
                       inventory.Product.SupplierName.ToLower().Contains(filterText) ||
                       inventory.Product.Colour.ToLower().Contains(filterText) ||
                       inventory.Product.Type.ToLower().Contains(filterText) ||
                       inventory.Product.ProductId.ToString().ToLower().Contains(filterText); ;
            }
            return false;
        }

        private void ProductTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SuggestionsListBox.Visibility = Visibility.Collapsed;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var inventoryitems = Inventories; // Replace with the actual list of products

            // Create a new Excel package
            using (var workbook = new XLWorkbook())
            {
                // Add a worksheet to the package
                var worksheet = workbook.Worksheets.Add("Inventory");

                // Add header row
                worksheet.Cell(1, 1).Value = "Inventory ID";
                worksheet.Cell(1, 2).Value = "Product ID";
                worksheet.Cell(1, 3).Value = "Product Name";
                worksheet.Cell(1, 4).Value = "Product Description";
                worksheet.Cell(1, 5).Value = "Stock";
                worksheet.Cell(1, 6).Value = "Purchase Date";
                worksheet.Cell(1, 7).Value = "Expiry Date";
                worksheet.Cell(1, 8).Value = "Buy Price";
                worksheet.Cell(1, 9).Value = "Sell Price";
                worksheet.Cell(1, 10).Value = "Units";
                worksheet.Cell(1, 11).Value = "UOM";
                worksheet.Cell(1, 12).Value = "Supplier Name";
                worksheet.Cell(1, 13).Value = "Type";
                worksheet.Cell(1, 14).Value = "Discount";
                worksheet.Cell(1, 15).Value = "Colour";
                worksheet.Cell(1, 16).Value = "Barcode";


                // Add product data to the sheet
                int row = 2;
                foreach (var item in inventoryitems)
                {
                    worksheet.Cell(row, 1).Value = item.InventoryId;
                    worksheet.Cell(row, 2).Value = item.ProductId;
                    worksheet.Cell(row, 3).Value = item.Product.Name;
                    worksheet.Cell(row, 4).Value = item.Product.Description;
                    worksheet.Cell(row, 5).Value = item.Quantity;
                    worksheet.Cell(row, 6).Value = item.PurchaseDate;
                    worksheet.Cell(row, 7).Value = item.ExpiryDate;
                    worksheet.Cell(row, 8).Value = item.Product.BuyPrice;
                    worksheet.Cell(row, 9).Value = item.Product.SellPrice;
                    worksheet.Cell(row, 10).Value = item.Product.Units;
                    worksheet.Cell(row, 11).Value = item.Product.UOM;
                    worksheet.Cell(row, 12).Value = item.Product.SupplierName;
                    worksheet.Cell(row, 13).Value = item.Product.Type;
                    worksheet.Cell(row, 14).Value = item.Product.Discount;
                    worksheet.Cell(row, 15).Value = item.Product.Colour;
                    worksheet.Cell(row, 16).Value = item.Product.Barcode;
                    row++;
                }

                // Save the Excel file
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = "Inventory.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var filePath = saveFileDialog.FileName;
                    workbook.SaveAs(filePath);
                    MessageBox.Show("Inventory exported successfully!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
