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
            var mode = parameter as string;

            if (string.Equals(mode, "AnyClicked", StringComparison.OrdinalIgnoreCase))
            {
                var clicked = values.Length > 0 ? values[0] as string : null;
                return !string.IsNullOrWhiteSpace(clicked) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (values.Length < 2) return Visibility.Collapsed;

            var clickedAnswer = values[0] as string;
            var answer = values[1] as string;

            return !string.IsNullOrWhiteSpace(clickedAnswer) && clickedAnswer == answer
                ? Visibility.Visible
                : Visibility.Collapsed;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
