
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Labb3_Quiz.Converters
{
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is bool b)
            {
                return b ? Visibility.Collapsed : Visibility.Visible; 
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
           => throw new NotImplementedException();
    }
}

