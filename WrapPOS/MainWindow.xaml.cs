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

namespace WrapPOS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    

    public partial class MainWindow : Window
    {
        private bool isCollapsed = false;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void Products_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Views.ProductPage());
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Views.InventoryPage());
        }

        private void Sales_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Views.SalesPage());
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Views.Dashboard());
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Views.Home());

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit?", "Exit Confirmation",
                                                      MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }


        private void ToggleSidebar(object sender, RoutedEventArgs e)
        {
            if (isCollapsed)
            {
                Sidebar.Width = 160;
                ProductsText.Visibility = Visibility.Visible;
                InventoryText.Visibility = Visibility.Visible;
                SalesText.Visibility = Visibility.Visible;
                HomeText.Visibility = Visibility.Visible;
                DashboardText.Visibility = Visibility.Visible;
                Logour.Visibility = Visibility.Visible;
            }
            else
            {
                Sidebar.Width = 45;
                ProductsText.Visibility = Visibility.Collapsed;
                InventoryText.Visibility = Visibility.Collapsed;
                SalesText.Visibility = Visibility.Collapsed;
                HomeText.Visibility = Visibility.Collapsed;
                DashboardText.Visibility = Visibility.Collapsed;
                Logour.Visibility = Visibility.Collapsed;

            }
            isCollapsed = !isCollapsed;
        }

    }
}
