using System.Globalization;
using System.Windows.Data;

namespace AdminTaskSystem.Converters;

public sealed class StatusCaptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value?.ToString() switch
        {
            "new" => "Новая",
            "in_progress" => "В работе",
            "done" => "Готово",
            "overdue" => "Просрочена",
            "rejected" => "Отклонена",
            _ => value?.ToString() ?? string.Empty
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
