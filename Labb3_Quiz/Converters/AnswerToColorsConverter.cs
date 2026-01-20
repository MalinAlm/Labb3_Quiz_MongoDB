
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Labb3_Quiz.Converters
{
    public class AnswerToColorsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
        
            if (values.Length < 3) return (Brush)new BrushConverter().ConvertFromString("#F3F3F3");

            var clickedAnswer = values[0] as string;
            var buttonText = values[1] as string;
            var correctAnswer = values[2] as string;

            if (string.IsNullOrEmpty(clickedAnswer)) return (Brush)new BrushConverter().ConvertFromString("#F3F3F3");

            //Correct answer
            if (buttonText == correctAnswer) return (Brush)new BrushConverter().ConvertFromString("#C8FFAC");

            //Incorrect answer
            if (buttonText == clickedAnswer) return (Brush)new BrushConverter().ConvertFromString("#FFA7A7");

            return Brushes.LightGray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
           => throw new NotImplementedException();
    }
}
