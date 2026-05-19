using System.Collections.ObjectModel;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Task = System.Threading.Tasks.Task;

namespace AdminTaskSystem.ViewModels;

public partial class TicketDetailViewModel : BaseViewModel
{
    private readonly int? ticketId;
    private readonly TicketService ticketService = new(App.SupabaseService);
    private readonly EmployeeService employeeService = new(App.SupabaseService);
    private string? originalStatus;

    public ObservableCollection<Category> Categories { get; } = [];
    public ObservableCollection<Department> Departments { get; } = [];
    public ObservableCollection<Employee> Employees { get; } = [];
    public ObservableCollection<Employee> FilteredEmployees { get; } = [];
    public ObservableCollection<TicketHistory> History { get; } = [];
    public IReadOnlyList<string> Statuses { get; } = ["new", "in_progress", "done", "overdue", "rejected"];
    public bool IsAdmin => App.CurrentUser?.IsAdmin == true;
    public bool IsEmployeeEditable => !IsAdmin && Ticket.AssignedEmployeeId == App.CurrentUser?.Id;
    public bool CanEditFields => IsAdmin || Ticket.Id == 0;

    [ObservableProperty]
    private Ticket ticket = new();

    [ObservableProperty]
    private string comment = string.Empty;

    public TicketDetailViewModel(int? ticketId)
    {
        this.ticketId = ticketId;
        Title = ticketId.HasValue ? "Редактирование заявки" : "Новая заявка";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            Categories.Clear();
            Departments.Clear();
            Employees.Clear();
            FilteredEmployees.Clear();
            History.Clear();

            foreach (var category in await employeeService.GetCategoriesAsync())
            {
                Categories.Add(category);
            }

            foreach (var department in await employeeService.GetDepartmentsAsync())
            {
                Departments.Add(department);
            }

            foreach (var employee in await employeeService.GetEmployeesAsync())
            {
                Employees.Add(employee);
            }

            if (ticketId.HasValue)
            {
                Ticket = await ticketService.GetTicketAsync(ticketId.Value) ?? new Ticket();
                originalStatus = Ticket.Status;
                foreach (var item in await ticketService.GetHistoryAsync(ticketId.Value))
                {
                    History.Add(item);
                }
            }
            else
            {
                Ticket = new Ticket
                {
                    Status = "new",
                    Deadline = DateTime.Now.AddDays(3),
                    CreatedById = App.CurrentUser?.Id,
                    AssignedDepartmentId = App.CurrentUser?.IsAdmin == true ? null : App.CurrentUser?.DepartmentId
                };
                originalStatus = Ticket.Status;
            }

            RefreshEmployees();
            OnPropertyChanged(nameof(IsEmployeeEditable));
            OnPropertyChanged(nameof(CanEditFields));
        }
        catch (Exception ex)
        {
            Notify($"Не удалось открыть заявку: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!Validate())
        {
            return;
        }

        if (!IsAdmin && Ticket.Id != 0 && Ticket.AssignedEmployeeId != App.CurrentUser?.Id)
        {
            Notify("Вы можете менять только назначенные вам заявки.");
            return;
        }

        IsBusy = true;
        try
        {
            if (!IsAdmin && Ticket.Id != 0)
            {
                var existing = await ticketService.GetTicketAsync(Ticket.Id);
                if (existing is not null)
                {
                    existing.Status = Ticket.Status;
                    Ticket = existing;
                }
            }

            if (Ticket.Id == 0)
            {
                Ticket.CreatedById = App.CurrentUser?.Id;
                Ticket = await ticketService.CreateAsync(Ticket);
            }
            else
            {
                Ticket = await ticketService.UpdateAsync(Ticket, originalStatus, App.CurrentUser?.Id ?? 0, Comment);
            }

            DialogHost.Close("RootDialog", true);
        }
        catch (Exception ex)
        {
            Notify($"Не удалось сохранить заявку: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel() => DialogHost.Close("RootDialog", false);

    [RelayCommand]
    private void DepartmentChanged()
    {
        RefreshEmployees();
        Ticket.AssignedEmployeeId = null;
    }

    private void RefreshEmployees()
    {
        FilteredEmployees.Clear();
        foreach (var employee in Employees.Where(x => Ticket.AssignedDepartmentId is null || x.DepartmentId == Ticket.AssignedDepartmentId))
        {
            FilteredEmployees.Add(employee);
        }
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Ticket.Title) ||
            string.IsNullOrWhiteSpace(Ticket.Description) ||
            Ticket.CategoryId is null ||
            string.IsNullOrWhiteSpace(Ticket.ApplicantName))
        {
            Notify("Заполните обязательные поля со звездочкой.");
            return false;
        }

        if (Ticket.Deadline is not null && Ticket.Deadline <= DateTime.Now)
        {
            Notify("Дедлайн должен быть больше текущей даты.");
            return false;
        }

        return true;
    }
}
