using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WrapPOS.Data;
using WrapPOS.Models;
using Microsoft.Win32;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Paragraph = iTextSharp.text.Paragraph;

namespace WrapPOS.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        public ObservableCollection<Grouping<string, Product>> GroupedProducts { get; set; }
        private DatabaseService _databaseService;
        List<string> TypeList = new List<string>();
        private List<string> _selectedTypes = new List<string>();
        private ObservableCollection<Product> _allProducts;
        private ObservableCollection<Product> CartProducts = new ObservableCollection<Product>();
        public SnackbarMessageQueue SnackbarMessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));

        public Home()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            LoadProducts();
            this.DataContext = this;
        }

        public List<string> ProductTypes { get; set; }

        private void LoadProducts()
        {
            _allProducts = _databaseService.GetProducts();
            ProductItemsControl.ItemsSource = _allProducts;

            TypeList = _allProducts.AsEnumerable().Select(o => o.Type).Distinct().ToList();

            Type_Itemcontroller.ItemsSource = TypeList;


            var grouped = _allProducts
                .GroupBy(p => p.Type)
                .Select(g => new Grouping<string, Product>(g.Key, g.ToList()))
                .ToList();

            GroupedProducts = new ObservableCollection<Grouping<string, Product>>(grouped);


            CartListBox.ItemsSource = CartProducts;
        }

        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                string type = checkBox.Content.ToString();

                if (checkBox.IsChecked == true)
                {
                    if (!_selectedTypes.Contains(type))
                        _selectedTypes.Add(type);
                }
                else
                {
                    _selectedTypes.Remove(type);
                }

                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            if (_selectedTypes.Count == 0)
            {
                ProductItemsControl.ItemsSource = _allProducts; // Show all if nothing is selected
            }
            else
            {
                ProductItemsControl.ItemsSource = _allProducts
                    .Where(p => _selectedTypes.Contains(p.Type))
                    .ToList();
            }
        }
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                AddToCart(product);
                SnackbarMessageQueue.Enqueue($"Added {product.Name} to cart.");
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                CartProducts.Remove(product);
                UpdateTotal();
            }
        }

        private void AddToCart(Product product)
        {
            CartProducts.Add(product);
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = CartProducts.Sum(p => p.SellPrice*p.Quantity);
            TotalAmountText.Text = $"Rs. {total:F2}";

        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                if (product.Quantity < product.Stock)  // Prevent exceeding stock
                {
                    product.Quantity++;
                    UpdateTotal();
                }
                else
                {
                    SnackbarMessageQueue.Enqueue("Cannot add more! Stock limit reached.");
                }
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Product product)
            {
                if (product.Quantity > 1) // Prevent reducing below 1
                {
                    product.Quantity--;
                    UpdateTotal();
                }
                else
                {
                    SnackbarMessageQueue.Enqueue("Minimum quantity is 1.");
                }
            }
        }

        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.Parse(CartProducts.Count().ToString()) == 0)
            {
                MessageBox.Show("Your cart is empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Save Invoice",
                FileName = "Invoice_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                GenerateInvoice(saveFileDialog.FileName);
                MessageBox.Show("Invoice saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GenerateInvoice(string filePath)
        {
            Document doc = new Document(PageSize.A4);
            try
            {
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                // Set fonts
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.BLACK);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

                // Add Logo
                string logoPath = "Resources/Logo.png"; // No leading slash
                Uri resourceUri = new Uri($"pack://application:,,,/{logoPath}");

                using (Stream stream = Application.GetResourceStream(resourceUri)?.Stream)
                {
                    if (stream != null)
                    {
                        byte[] imageData;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            imageData = ms.ToArray();
                        }

                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imageData);
                        logo.ScaleToFit(100, 100);
                        logo.Alignment = Element.ALIGN_CENTER;
                        doc.Add(logo);
                    }
                }

                // Business Name & Tagline
                //Paragraph businessName = new Paragraph("Gift With Love", titleFont);
                //businessName.Alignment = Element.ALIGN_CENTER;
                //doc.Add(businessName);
                doc.Add(new Paragraph("Your perfect gifts, wrapped with love\n\n", textFont) { Alignment = Element.ALIGN_CENTER });

                // Invoice Title
                Paragraph title = new Paragraph("INVOICE", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                doc.Add(title);
                doc.Add(new Paragraph("\n"));

                // Invoice Details
                PdfPTable detailsTable = new PdfPTable(2);
                detailsTable.WidthPercentage = 100;
                detailsTable.SetWidths(new float[] { 50f, 50f });

                detailsTable.AddCell(new PdfPCell(new Phrase($"Invoice No: #000123", textFont)) { Border = 0 });
                detailsTable.AddCell(new PdfPCell(new Phrase($"Date: {DateTime.Now:yyyy-MM-dd}", textFont)) { Border = 0 });

                detailsTable.AddCell(new PdfPCell(new Phrase("Customer: John Doe", textFont)) { Border = 0 });
                detailsTable.AddCell(new PdfPCell(new Phrase("Contact: +94 712345678", textFont)) { Border = 0 });

                doc.Add(detailsTable);
                doc.Add(new Paragraph("\n"));

                // Define Colors
                BaseColor headerColor = new BaseColor(173, 216, 230); // Light Blue
                BaseColor rowAlternateColor = new BaseColor(240, 240, 240); // Light Gray

                // Product Table
                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 25f, 25f, 10f, 10f, 15f, 15f });

                // Table Headers
                string[] headers = { "Product", "Description", "Units", "UOM", "Unit Price", "Total" };
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = headerColor,
                        Padding = 5,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }

                decimal totalAmount = 0;
                bool alternateRow = false; // Toggle row colors

                foreach (var product in CartProducts)
                {
                    BaseColor rowColor = alternateRow ? rowAlternateColor : BaseColor.WHITE;
                    alternateRow = !alternateRow;

                    PdfPCell nameCell = new PdfPCell(new Phrase(product.Name, textFont)) { BackgroundColor = rowColor, Padding = 5 };
                    PdfPCell descCell = new PdfPCell(new Phrase(product.Description, textFont)) { BackgroundColor = rowColor, Padding = 5 };
                    PdfPCell unitsCell = new PdfPCell(new Phrase(product.Quantity.ToString(), textFont)) { BackgroundColor = rowColor, Padding = 5 };
                    PdfPCell uomCell = new PdfPCell(new Phrase(product.UOM, textFont)) { BackgroundColor = rowColor, Padding = 5 };
                    PdfPCell priceCell = new PdfPCell(new Phrase("Rs. " + product.SellPrice.ToString("F2"), textFont)) { BackgroundColor = rowColor, Padding = 5 };
                    PdfPCell totalCell = new PdfPCell(new Phrase("Rs. " + (product.SellPrice * product.Quantity).ToString("F2"), textFont)) { BackgroundColor = rowColor, Padding = 5 };

                    table.AddCell(nameCell);
                    table.AddCell(descCell);
                    table.AddCell(unitsCell);
                    table.AddCell(uomCell);
                    table.AddCell(priceCell);
                    table.AddCell(totalCell);

                    totalAmount += product.SellPrice * product.Quantity;
                }

                doc.Add(table);
                doc.Add(new Paragraph("\n"));

                // Total Amount Calculation
                decimal tax = totalAmount * 0.05m; // 5% Tax
                decimal grandTotal = totalAmount + tax;

                PdfPTable totalTable = new PdfPTable(2);
                totalTable.WidthPercentage = 100;
                totalTable.SetWidths(new float[] { 70f, 30f });

                totalTable.AddCell(new PdfPCell(new Phrase("Subtotal:", textFont)) { Border = 0, Padding = 5 });
                totalTable.AddCell(new PdfPCell(new Phrase("Rs. " + totalAmount.ToString("F2"), textFont)) { Border = 0, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });

                totalTable.AddCell(new PdfPCell(new Phrase("Tax (5%):", textFont)) { Border = 0, Padding = 5 });
                totalTable.AddCell(new PdfPCell(new Phrase("Rs. " + tax.ToString("F2"), textFont)) { Border = 0, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });

                PdfPCell grandTotalCellLabel = new PdfPCell(new Phrase("Grand Total:", headerFont))
                {
                    Border = 0,
                    Padding = 5,
                    BackgroundColor = headerColor
                };

                PdfPCell grandTotalCellValue = new PdfPCell(new Phrase("Rs. " + grandTotal.ToString("F2"), headerFont))
                {
                    Border = 0,
                    Padding = 5,
                    BackgroundColor = headerColor,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };

                totalTable.AddCell(grandTotalCellLabel);
                totalTable.AddCell(grandTotalCellValue);

                doc.Add(totalTable);
                doc.Add(new Paragraph("\n"));

                // Footer - Thank You Message
                Paragraph thankYou = new Paragraph("Thank you for shopping with us!\nWebsite: www.giftwithlove.com | Contact: +94 712345678", textFont);
                thankYou.Alignment = Element.ALIGN_CENTER;
                doc.Add(thankYou);
                doc.Add(new Paragraph("\n"));

                // Generated By Message
                Paragraph generatedBy = new Paragraph("Generated by WrapPOS System", textFont);
                generatedBy.Alignment = Element.ALIGN_CENTER;
                doc.Add(generatedBy);
                doc.Add(new Paragraph("\n"));

                // Signature Area
                PdfPTable signatureTable = new PdfPTable(1);
                signatureTable.WidthPercentage = 50;
                signatureTable.HorizontalAlignment = Element.ALIGN_RIGHT;

                signatureTable.AddCell(new PdfPCell(new Phrase("\n\nAuthorized Signature", textFont))
                {
                    Border = 0,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingBottom = 20
                });

                // Add Signature Image
                string signaturePath = "Resources/signature.png"; 
                Uri signatureUri = new Uri($"pack://application:,,,/{signaturePath}");

                using (Stream stream = Application.GetResourceStream(signatureUri)?.Stream)
                {
                    if (stream != null)
                    {
                        byte[] imageData;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            imageData = ms.ToArray();
                        }

                        iTextSharp.text.Image signature = iTextSharp.text.Image.GetInstance(imageData);
                        signature.ScaleToFit(100, 100);
                        signature.WidthPercentage = 50;
                        signature.Alignment = Element.ALIGN_RIGHT;
                        doc.Add(signature);
                    }
                }

                doc.Add(signatureTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating invoice: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                doc.Close();
            }
        }
    }

        public class Grouping<TKey, TValue> : ObservableCollection<TValue>
        {
        public TKey Key { get; private set; }

        public Grouping(TKey key, IEnumerable<TValue> items) : base(items)
        {
            Key = key;
        }
    }



}
