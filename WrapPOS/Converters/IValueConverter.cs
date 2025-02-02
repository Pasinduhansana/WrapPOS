using System;
using System.Globalization;
using System.Windows.Data;

namespace WrapPOS.Converters // Replace with your actual namespace
{
    public class ProportionalWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double listViewWidth && values[1] is int weight)
            {
                double totalWeight = 8; // Sum of all weights (1 + 2 + 1 + 1 + 1 + 1)
                return (listViewWidth / totalWeight) * weight;
            }
            return 0; // Fallback value
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}