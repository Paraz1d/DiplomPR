using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AdminTaskSystem.Converters;

public sealed class RoleToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var expected = parameter?.ToString() ?? "admin";
        return string.Equals(value?.ToString(), expected, StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
