using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AdminTaskSystem.Infrastructure;

public static class EventCommandBehavior
{
    public static readonly DependencyProperty LoadedCommandProperty =
        DependencyProperty.RegisterAttached("LoadedCommand", typeof(ICommand), typeof(EventCommandBehavior),
            new PropertyMetadata(null, OnLoadedCommandChanged));

    public static ICommand? GetLoadedCommand(DependencyObject obj) => (ICommand?)obj.GetValue(LoadedCommandProperty);
    public static void SetLoadedCommand(DependencyObject obj, ICommand value) => obj.SetValue(LoadedCommandProperty, value);

    public static readonly DependencyProperty RowDoubleClickCommandProperty =
        DependencyProperty.RegisterAttached("RowDoubleClickCommand", typeof(ICommand), typeof(EventCommandBehavior),
            new PropertyMetadata(null, OnRowDoubleClickCommandChanged));

    public static ICommand? GetRowDoubleClickCommand(DependencyObject obj) => (ICommand?)obj.GetValue(RowDoubleClickCommandProperty);
    public static void SetRowDoubleClickCommand(DependencyObject obj, ICommand value) => obj.SetValue(RowDoubleClickCommandProperty, value);

    private static void OnLoadedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        element.Loaded -= ExecuteLoadedCommand;
        if (e.NewValue is ICommand)
        {
            element.Loaded += ExecuteLoadedCommand;
        }
    }

    private static void ExecuteLoadedCommand(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            var command = GetLoadedCommand(element);
            if (command?.CanExecute(null) == true)
            {
                command.Execute(null);
            }
        }
    }

    private static void OnRowDoubleClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid)
        {
            return;
        }

        grid.MouseDoubleClick -= ExecuteRowDoubleClickCommand;
        if (e.NewValue is ICommand)
        {
            grid.MouseDoubleClick += ExecuteRowDoubleClickCommand;
        }
    }

    private static void ExecuteRowDoubleClickCommand(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid grid || grid.SelectedItem is null)
        {
            return;
        }

        var command = GetRowDoubleClickCommand(grid);
        if (command?.CanExecute(grid.SelectedItem) == true)
        {
            command.Execute(grid.SelectedItem);
        }
    }
}
