using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using AdminTaskSystem.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AdminTaskSystem.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly NotificationService notificationService = new();
    private readonly DispatcherTimer timer;

    [ObservableProperty] private BaseViewModel currentViewModel = new ProfileViewModel();
    [ObservableProperty] private int notificationCount;
    [ObservableProperty] private bool hasNotifications;
    [ObservableProperty] private ObservableCollection<Notification> notifications = new();
    [ObservableProperty] private bool isNotificationPanelOpen;
    [ObservableProperty] private bool isOffline = !CacheService.IsOnline;
    [ObservableProperty] private bool isProfileSelected = true;
    [ObservableProperty] private bool isTicketsSelected;
    [ObservableProperty] private bool isTasksSelected;
    [ObservableProperty] private bool isEmployeesSelected;
    [ObservableProperty] private bool isReportsSelected;

    public bool ShowEmployees => AppSession.IsAdmin;
    public bool ShowReports => AppSession.CanManage;
    public string CurrentUserName => AppSession.CurrentUser?.FullName ?? string.Empty;
    public string CurrentUserInitials => AppSession.Initials;
    public string RoleBadgeText => AppSession.RoleName;
    public string CurrentPageTitle => CurrentViewModel.Title;

    public MainViewModel()
    {
        Title = "Главная";
        _ = LoadCurrentAsync();
        _ = RefreshNotificationsAsync();
        timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
        timer.Tick += async (_, _) => await RefreshNotificationsAsync();
        timer.Start();
    }

    partial void OnCurrentViewModelChanged(BaseViewModel value) => OnPropertyChanged(nameof(CurrentPageTitle));

    [RelayCommand]
    private async Task NavigateToProfileAsync()
    {
        Select("profile");
        CurrentViewModel = new ProfileViewModel();
        await LoadCurrentAsync();
    }

    [RelayCommand]
    private async Task NavigateToTicketsAsync()
    {
        Select("tickets");
        CurrentViewModel = new TicketsViewModel();
        await LoadCurrentAsync();
    }

    [RelayCommand]
    private async Task NavigateToTasksAsync()
    {
        Select("tasks");
        CurrentViewModel = new TasksViewModel();
        await LoadCurrentAsync();
    }

    [RelayCommand]
    private async Task NavigateToEmployeesAsync()
    {
        if (!AppSession.IsAdmin) return;
        Select("employees");
        CurrentViewModel = new EmployeesViewModel();
        await LoadCurrentAsync();
    }

    [RelayCommand]
    private async Task NavigateToReportsAsync()
    {
        if (!AppSession.CanManage) return;
        Select("reports");
        CurrentViewModel = new ReportsViewModel();
        await LoadCurrentAsync();
    }

    [RelayCommand]
    private void ToggleNotifications() => IsNotificationPanelOpen = !IsNotificationPanelOpen;

    [RelayCommand]
    private async Task MarkAllReadAsync()
    {
        try
        {
            await notificationService.MarkAllReadAsync();
            await RefreshNotificationsAsync();
        }
        catch (Exception ex)
        {
            NotifyError(ex.Message);
        }
    }

    [RelayCommand]
    private void Logout()
    {
        timer.Stop();
        AppSession.Clear();
        new LoginWindow().Show();
        Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
    }

    private async Task RefreshNotificationsAsync()
    {
        try
        {
            Notifications = new ObservableCollection<Notification>(await notificationService.GetUnreadAsync());
            NotificationCount = Notifications.Count;
            HasNotifications = NotificationCount > 0;
        }
        catch
        {
            Notifications = new ObservableCollection<Notification>();
            NotificationCount = 0;
            HasNotifications = false;
        }
    }

    private async Task LoadCurrentAsync()
    {
        switch (CurrentViewModel)
        {
            case ProfileViewModel vm: await vm.LoadAsync(); break;
            case TicketsViewModel vm: await vm.LoadAsync(); break;
            case TasksViewModel vm: await vm.LoadAsync(); break;
            case EmployeesViewModel vm: await vm.LoadAsync(); break;
            case ReportsViewModel vm: await vm.LoadAsync(); break;
        }
    }

    private void Select(string key)
    {
        IsProfileSelected = key == "profile";
        IsTicketsSelected = key == "tickets";
        IsTasksSelected = key == "tasks";
        IsEmployeesSelected = key == "employees";
        IsReportsSelected = key == "reports";
    }
}
