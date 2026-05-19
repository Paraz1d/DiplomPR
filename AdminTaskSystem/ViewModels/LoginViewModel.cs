using System.Windows;
using AdminTaskSystem.Services;
using AdminTaskSystem.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AdminTaskSystem.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService authService = new();

    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string errorMessage = string.Empty;
    [ObservableProperty] private bool hasError;

    public LoginViewModel()
    {
        Title = "Вход";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;
        try
        {
            var employee = await authService.LoginAsync(Email, Password);
            if (employee is null)
            {
                HasError = true;
                ErrorMessage = "Неверный email или пароль";
                return;
            }

            AppSession.CurrentUser = employee;
            if (CacheService.IsOnline)
            {
                await SupabaseService.Client.Rpc("fn_notify_overdue", new Dictionary<string, object>());
            }

            new MainWindow { DataContext = new MainViewModel() }.Show();
            Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault()?.Close();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
