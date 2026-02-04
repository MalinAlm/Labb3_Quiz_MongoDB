using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Labb3_Quiz.Converters
{
    public class AnswerEqualsClickedToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Visibility.Collapsed;

            var clicked = values[0] as string;
            var answer = values[1] as string;

            return !string.IsNullOrEmpty(clicked) && clicked == answer
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
