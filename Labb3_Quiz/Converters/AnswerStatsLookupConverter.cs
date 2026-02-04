using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Labb3_Quiz.Converters
{
    public class AnswerStatsLookupConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return "";

            if (values[0] is not IDictionary<string, int> dict) return "";

            var answerText = values[1] as string;
            if (string.IsNullOrWhiteSpace(answerText)) return "";

            dict.TryGetValue(answerText, out var count);
            return $"Prevoius players picked {answerText}, {count} times";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
