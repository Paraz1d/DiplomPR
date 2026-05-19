using System.Collections.ObjectModel;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace AdminTaskSystem.ViewModels;

public partial class TaskDetailViewModel : BaseViewModel
{
    private readonly TaskFull? original;
    private readonly TaskService taskService = new();
    private readonly TicketService ticketService = new();
    private readonly EmployeeService employeeService = new();

    [ObservableProperty] private string taskTitle = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private TicketFull? selectedTicket;
    [ObservableProperty] private Employee? selectedEmployee;
    [ObservableProperty] private DateTime? deadline;
    [ObservableProperty] private string selectedStatus = "new";

    public ObservableCollection<TicketFull> Tickets { get; } = [];
    public ObservableCollection<Employee> Employees { get; } = [];
    public List<string> StatusOptions { get; } = ["new", "in_progress", "done", "overdue"];
    public bool CanManage => AppSession.CanManage;
    public bool CanChangeStatus => AppSession.CanManage || (AppSession.IsEmployee && original?.AssignedEmployeeId == AppSession.CurrentUser?.Id);
    public bool IsNewTask => original is null;
    public string HeaderTitle => IsNewTask ? "Новая задача" : $"Задача #{original!.Id}";

    public TaskDetailViewModel(TaskFull? task)
    {
        original = task;
        Title = HeaderTitle;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            Tickets.Clear(); Employees.Clear();
            foreach (var ticket in await ticketService.GetTicketsAsync()) Tickets.Add(ticket);
            foreach (var employee in await employeeService.GetAllAsync()) Employees.Add(employee);
            if (original is not null)
            {
                TaskTitle = original.Title;
                Description = original.Description ?? string.Empty;
                SelectedTicket = Tickets.FirstOrDefault(x => x.Id == original.TicketId);
                SelectedEmployee = Employees.FirstOrDefault(x => x.Id == original.AssignedEmployeeId);
                Deadline = original.Deadline;
                SelectedStatus = original.Status;
            }
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(TaskTitle)) { NotifyError("Заполните обязательные поля"); return; }
        if (Deadline.HasValue && Deadline.Value < DateTime.Now) { NotifyError("Дедлайн не может быть в прошлом"); return; }
        try
        {
            var task = new TaskItem
            {
                Id = original?.Id ?? 0,
                Title = TaskTitle.Trim(),
                Description = Description,
                TicketId = SelectedTicket?.Id,
                AssignedEmployeeId = SelectedEmployee?.Id,
                Deadline = Deadline,
                Status = SelectedStatus,
                CreatedAt = original?.CreatedAt,
                CreatedById = original?.CreatedById
            };
            if (IsNewTask) await taskService.CreateTaskAsync(task); else await taskService.UpdateTaskAsync(task);
            DialogHost.Close("RootDialog", true);
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
    }

    [RelayCommand]
    private void Close() => DialogHost.Close("RootDialog", false);
}
