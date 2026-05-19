using AdminTaskSystem.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AdminTaskSystem.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    public Employee CurrentUser { get; }
    public bool IsAdmin => CurrentUser.IsAdmin;

    [ObservableProperty]
    private BaseViewModel currentPage;

    public MainViewModel(Employee currentUser)
    {
        CurrentUser = currentUser;
        Title = "AdminTaskSystem";
        currentPage = new TicketsViewModel();
    }

    [RelayCommand]
    private void ShowTickets() => CurrentPage = new TicketsViewModel();

    [RelayCommand]
    private void ShowTasks() => CurrentPage = new TasksViewModel();

    [RelayCommand]
    private void ShowEmployees()
    {
        if (IsAdmin)
        {
            CurrentPage = new EmployeesViewModel();
        }
    }

    [RelayCommand]
    private void ShowReports()
    {
        if (IsAdmin)
        {
            CurrentPage = new ReportsViewModel();
        }
    }

    [RelayCommand]
    private void Logout()
    {
        App.CurrentUser = null;
        var login = new Views.LoginWindow();
        login.Show();

        foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
        {
            if (window is Views.MainWindow)
            {
                window.Close();
                break;
            }
        }
    }
}
