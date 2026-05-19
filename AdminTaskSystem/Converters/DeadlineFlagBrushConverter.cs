using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AdminTaskSystem.Converters;

public sealed class DeadlineFlagBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value?.ToString() switch
    {
        "overdue" => Brush("#FFF5F5"),
        "warning" => Brush("#FFFBF0"),
        _ => Brush("#FFFFFF")
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    private static SolidColorBrush Brush(string hex) => new((Color)ColorConverter.ConvertFromString(hex));
}
