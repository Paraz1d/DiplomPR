using System.Globalization;
using System.Windows.Data;

namespace AdminTaskSystem.Converters;

public sealed class StatusCaptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value?.ToString() switch
    {
        "new" => "Новая",
        "in_progress" => "В работе",
        "done" => "Выполнена",
        "overdue" => "Просрочена",
        "rejected" => "Отклонена",
        _ => value?.ToString() ?? string.Empty
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Новая" => "new",
            "В работе" => "in_progress",
            "Выполнена" => "done",
            "Просрочена" => "overdue",
            "Отклонена" => "rejected",
            _ => value?.ToString() ?? string.Empty
        };
    }
}
