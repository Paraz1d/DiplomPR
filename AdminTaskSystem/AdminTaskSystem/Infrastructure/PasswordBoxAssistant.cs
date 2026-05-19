using System.Windows;
using System.Windows.Controls;

namespace AdminTaskSystem.Infrastructure;

public static class PasswordBoxAssistant
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxAssistant),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

    public static string GetBoundPassword(DependencyObject obj) => (string)obj.GetValue(BoundPasswordProperty);
    public static void SetBoundPassword(DependencyObject obj, string value) => obj.SetValue(BoundPasswordProperty, value);

    private static readonly DependencyProperty UpdatingPasswordProperty =
        DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordBoxAssistant));

    private static bool GetUpdatingPassword(DependencyObject obj) => (bool)obj.GetValue(UpdatingPasswordProperty);
    private static void SetUpdatingPassword(DependencyObject obj, bool value) => obj.SetValue(UpdatingPasswordProperty, value);

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
        {
            return;
        }

        passwordBox.PasswordChanged -= HandlePasswordChanged;
        if (!GetUpdatingPassword(passwordBox))
        {
            passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
        }

        passwordBox.PasswordChanged += HandlePasswordChanged;
    }

    private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
        {
            return;
        }

        SetUpdatingPassword(passwordBox, true);
        SetBoundPassword(passwordBox, passwordBox.Password);
        SetUpdatingPassword(passwordBox, false);
    }
}
