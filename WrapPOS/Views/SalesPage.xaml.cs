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

namespace WrapPOS.Views
{
    /// <summary>
    /// Interaction logic for Sales.xaml
    /// </summary>
    public partial class SalesPage : Page
    {
        Sales sales = new Sales();
        public ObservableCollection<Sales> SalesList { get; set; }
        private static string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pos_database.db");
        private readonly DatabaseService _databaseService = new DatabaseService();
        private CollectionViewSource CollectionView;

        public SalesPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            SalesList = new ObservableCollection<Sales>();
            CollectionView = new CollectionViewSource { Source = SalesList };
            LoadSalesData();
        }

        private void LoadSalesData()
        {
            var SalesFromDb = _databaseService.GetSales();
            try
            {

                Application.Current.Dispatcher.Invoke(() =>
                {
                    SalesList.Clear();
                    foreach (var item in SalesFromDb)
                    {
                        SalesList.Add(item);
                    }

                });

                // Bind the data to update the ListView
                SalesListView.ItemsSource = SalesList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading sales data: " + ex.Message);
            }
        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
