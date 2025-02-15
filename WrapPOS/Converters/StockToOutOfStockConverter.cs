using System;
using System.Globalization;
using System.Windows.Data;

namespace WrapPOS.Converters
{
    public class StockToOutOfStockConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stock)
            {
                return stock == 0 ? "Out of Stock" : stock.ToString() + "";
            }
            return "Out of Stock";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}