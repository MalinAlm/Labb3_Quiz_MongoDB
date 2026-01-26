using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Labb3_Quiz.Converters
{
    public class AnswerToColorsConverter : IMultiValueConverter
    {
        private static readonly Brush DefaultBorder =
            (Brush)new BrushConverter().ConvertFromString("#33FFFFFF"); // samma som button-style

        private static readonly Brush CorrectBorder =
            (Brush)new BrushConverter().ConvertFromString("#7CFF6B"); // grön men lite neon

        private static readonly Brush IncorrectBorder =
            (Brush)new BrushConverter().ConvertFromString("#FF4C4C"); // röd

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3) return DefaultBorder;

            var clickedAnswer = values[0] as string;
            var buttonText = values[1] as string;
            var correctAnswer = values[2] as string;

            // innan man svarat -> default border
            if (string.IsNullOrWhiteSpace(clickedAnswer))
                return DefaultBorder;

            // rätt svar -> grön border
            if (buttonText == correctAnswer)
                return CorrectBorder;

            // klickat fel -> röd border
            if (buttonText == clickedAnswer)
                return IncorrectBorder;

            // övriga -> default border
            return DefaultBorder;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
