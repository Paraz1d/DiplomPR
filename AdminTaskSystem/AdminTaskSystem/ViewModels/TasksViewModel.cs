using System.Collections.ObjectModel;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using AdminTaskSystem.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using DbTask = AdminTaskSystem.Models.Task;
using Task = System.Threading.Tasks.Task;

namespace AdminTaskSystem.ViewModels;

public partial class TasksViewModel : BaseViewModel
{
    private readonly TaskService taskService = new(App.SupabaseService);
    private List<TaskFull> allTasks = [];

    public ObservableCollection<TaskFull> Tasks { get; } = [];
    public IReadOnlyList<string> Statuses { get; } = ["new", "in_progress", "done", "overdue"];

    [ObservableProperty]
    private TaskFull? selectedTask;

    [ObservableProperty]
    private string editStatus = "new";

    [ObservableProperty]
    private string editComment = string.Empty;

    public TasksViewModel()
    {
        Title = "Мои задачи";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (App.CurrentUser is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            allTasks = (await taskService.GetTasksAsync(App.CurrentUser)).ToList();
            Tasks.Clear();
            foreach (var task in allTasks)
            {
                Tasks.Add(task);
            }
        }
        catch (Exception ex)
        {
            Notify($"Не удалось загрузить задачи: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenTaskAsync(TaskFull task)
    {
        SelectedTask = task;
        EditStatus = task.Status;
        EditComment = string.Empty;
        await DialogHost.Show(new TaskEditDialog { DataContext = this }, "RootDialog");
        await LoadAsync();
    }

    [RelayCommand]
    private async Task SaveTaskAsync()
    {
        if (SelectedTask is null)
        {
            return;
        }

        var dbTask = await taskService.GetTaskAsync(SelectedTask.Id) ?? new DbTask { Id = SelectedTask.Id };
        if (App.CurrentUser?.IsAdmin != true && dbTask.AssignedEmployeeId != App.CurrentUser?.Id)
        {
            Notify("Вы можете менять только назначенные вам задачи.");
            return;
        }

        dbTask.Status = EditStatus;
        await taskService.SaveAsync(dbTask);
        DialogHost.Close("RootDialog", true);
        Notify("Задача обновлена.");
    }

    [RelayCommand]
    private void CancelEdit() => DialogHost.Close("RootDialog", false);
}
