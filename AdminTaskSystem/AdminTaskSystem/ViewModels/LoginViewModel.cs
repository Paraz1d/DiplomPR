using System.Windows;
using AdminTaskSystem.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Task = System.Threading.Tasks.Task;

namespace AdminTaskSystem.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isPasswordVisible;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasError;

    public LoginViewModel()
    {
        Title = "AdminTaskSystem";
    }

    [RelayCommand]
    private async Task LoginAsync(Window window)
    {
        HasError = false;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Введите email и пароль.");
            return;
        }

        try
        {
            IsBusy = true;
            var user = await App.AuthService.LoginAsync(Email.Trim(), Password);
            if (user is null)
            {
                ShowError("Неверный email или пароль.");
                return;
            }

            App.CurrentUser = user;
            new MainWindow { DataContext = new MainViewModel(user) }.Show();
            window.Close();
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка входа: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }
}
