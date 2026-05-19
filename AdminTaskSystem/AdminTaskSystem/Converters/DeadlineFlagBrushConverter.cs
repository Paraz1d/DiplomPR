using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AdminTaskSystem.Converters;

public sealed class DeadlineFlagBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value?.ToString() switch
        {
            "overdue" => new SolidColorBrush(Color.FromRgb(255, 235, 238)),
            "warning" => new SolidColorBrush(Color.FromRgb(255, 253, 231)),
            _ => Brushes.White
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
