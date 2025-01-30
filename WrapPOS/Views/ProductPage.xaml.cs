using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace WrapPOS.Views
{
    /// <summary>
    /// Interaction logic for ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public ProductPage()
        {
            InitializeComponent();
        }
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add Product Clicked");
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Delete Product Clicked");
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
                }

            }
            catch (Exception e1)
            {

                MessageBox.Show(e1.Message + ". Please Try again !", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            }
        }
    }
}
