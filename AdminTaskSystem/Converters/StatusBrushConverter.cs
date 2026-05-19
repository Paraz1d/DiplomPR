using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AdminTaskSystem.Converters;

public sealed class StatusBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value?.ToString() switch
    {
        "new" => Brush("#E3F2FD"),
        "in_progress" => Brush("#FFF3E0"),
        "done" => Brush("#E8F5E9"),
        "overdue" => Brush("#FFEBEE"),
        "rejected" => Brush("#F5F5F5"),
        _ => Brush("#ECEFF1")
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    private static SolidColorBrush Brush(string hex) => new((Color)ColorConverter.ConvertFromString(hex));
}
