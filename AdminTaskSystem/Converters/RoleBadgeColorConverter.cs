using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AdminTaskSystem.Converters;

public sealed class RoleBadgeColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value?.ToString() switch
    {
        "admin" => Brush("#7B1FA2"),
        "manager" => Brush("#1565C0"),
        "employee" => Brush("#2E7D32"),
        _ => Brush("#607D8B")
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    private static SolidColorBrush Brush(string hex) => new((Color)ColorConverter.ConvertFromString(hex));
}
