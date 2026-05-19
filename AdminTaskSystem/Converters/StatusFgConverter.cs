using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AdminTaskSystem.Converters;

public sealed class StatusFgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value?.ToString() switch
    {
        "new" => Brush("#0D47A1"),
        "in_progress" => Brush("#BF360C"),
        "done" => Brush("#1B5E20"),
        "overdue" => Brush("#B71C1C"),
        "rejected" => Brush("#424242"),
        _ => Brush("#37474F")
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    private static SolidColorBrush Brush(string hex) => new((Color)ColorConverter.ConvertFromString(hex));
}
