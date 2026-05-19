using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AdminTaskSystem.Converters;

public sealed class StatusBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value?.ToString() switch
        {
            "new" => new SolidColorBrush(Color.FromRgb(25, 118, 210)),
            "in_progress" => new SolidColorBrush(Color.FromRgb(245, 124, 0)),
            "done" => new SolidColorBrush(Color.FromRgb(46, 125, 50)),
            "overdue" => new SolidColorBrush(Color.FromRgb(198, 40, 40)),
            "rejected" => new SolidColorBrush(Color.FromRgb(97, 97, 97)),
            _ => new SolidColorBrush(Color.FromRgb(69, 90, 100))
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
