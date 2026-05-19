using System.Collections.ObjectModel;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using AdminTaskSystem.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace AdminTaskSystem.ViewModels;

public partial class TasksViewModel : BaseViewModel
{
    private readonly TaskService taskService = new();

    [ObservableProperty] private ObservableCollection<TaskFull> allTasks = new();
    [ObservableProperty] private ObservableCollection<TaskFull> filteredTasks = new();
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private string selectedStatusFilter = "Все";

    public List<string> StatusFilters { get; } = ["Все", "Новая", "В работе", "Выполнена", "Просрочена"];
    public bool CanManage => AppSession.CanManage;
    public bool CanUseFilters => AppSession.IsAdmin;

    public TasksViewModel() { Title = "Мои задачи"; }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedStatusFilterChanged(string value) => ApplyFilters();

    private void ApplyFilters()
    {
        var status = SelectedStatusFilter switch
        {
            "Новая" => "new",
            "В работе" => "in_progress",
            "Выполнена" => "done",
            "Просрочена" => "overdue",
            _ => string.Empty
        };
        var query = AllTasks.AsEnumerable();
        if (CanUseFilters)
        {
            if (!string.IsNullOrEmpty(status)) query = query.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(SearchText)) query = query.Where(x => x.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }
        FilteredTasks = new ObservableCollection<TaskFull>(query);
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            AllTasks = new ObservableCollection<TaskFull>(await taskService.GetTasksAsync());
            ApplyFilters();
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task OpenTaskAsync(TaskFull? task)
    {
        var vm = new TaskDetailViewModel(task);
        await DialogHost.Show(new TaskDetailDialog { DataContext = vm }, "RootDialog");
        await LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanManage))]
    private async Task CreateTaskAsync() => await OpenTaskAsync(null);
}
