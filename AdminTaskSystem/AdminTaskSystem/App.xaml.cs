using System.Windows;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using AdminTaskSystem.Views;

namespace AdminTaskSystem;

public partial class App : Application
{
    public static Employee? CurrentUser { get; set; }
    public static SupabaseService SupabaseService { get; private set; } = null!;
    public static AuthService AuthService { get; private set; } = null!;

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        SupabaseService = new SupabaseService();
        AuthService = new AuthService(SupabaseService);

        try
        {
            await SupabaseService.InitializeAsync();
            await SupabaseService.MarkOverdueAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось подключиться к Supabase: {ex.Message}", "AdminTaskSystem",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        new LoginWindow().Show();
    }
}
