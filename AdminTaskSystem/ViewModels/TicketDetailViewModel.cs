using System.Collections.ObjectModel;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace AdminTaskSystem.ViewModels;

public partial class TicketDetailViewModel : BaseViewModel
{
    private readonly TicketFull? original;
    private readonly TicketService ticketService = new();
    private readonly EmployeeService employeeService = new();

    [ObservableProperty] private string ticketTitle = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private Category? selectedCategory;
    [ObservableProperty] private Department? selectedDepartment;
    [ObservableProperty] private Employee? selectedEmployee;
    [ObservableProperty] private DateTime? deadline;
    [ObservableProperty] private string selectedStatus = "new";
    [ObservableProperty] private string applicantName = string.Empty;
    [ObservableProperty] private string applicantContact = string.Empty;
    [ObservableProperty] private string applicantAddress = string.Empty;
    [ObservableProperty] private string saveComment = string.Empty;

    public ObservableCollection<Category> Categories { get; } = [];
    public ObservableCollection<Department> Departments { get; } = [];
    public ObservableCollection<Employee> Employees { get; } = [];
    public ObservableCollection<Employee> AvailableEmployees { get; } = [];
    public ObservableCollection<TicketHistory> History { get; } = [];
    public List<string> StatusOptions { get; } = ["new", "in_progress", "done", "overdue", "rejected"];

    public bool IsNewTicket => original == null;
    public bool CanManage => AppSession.CanManage;
    public bool IsAdmin => AppSession.IsAdmin;
    public bool IsReadOnly => original?.IsLocked ?? false;
    public bool CanChangeStatus => AppSession.IsAdmin || AppSession.IsManager || (AppSession.IsEmployee && original?.AssignedEmployeeId == AppSession.CurrentUser?.Id);
    public bool CanAccept => original?.CanAccept ?? false;
    public bool CanSave => CanManage || CanChangeStatus;
    public string HeaderTitle => IsNewTicket ? "Новая заявка" : $"Заявка #{original!.Id} — {original.Title}";
    public string HeaderSubtitle => IsNewTicket ? "Создание новой заявки" : $"Создана: {original!.CreatedAt:dd.MM.yyyy HH:mm}";

    public TicketDetailViewModel(TicketFull? ticket)
    {
        original = ticket;
        Title = HeaderTitle;
    }

    partial void OnSelectedDepartmentChanged(Department? value) => FilterEmployees();

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            Categories.Clear(); Departments.Clear(); Employees.Clear(); AvailableEmployees.Clear(); History.Clear();
            foreach (var category in await employeeService.GetCategoriesAsync()) Categories.Add(category);
            foreach (var department in await employeeService.GetDepartmentsAsync()) Departments.Add(department);
            foreach (var employee in await employeeService.GetAllAsync()) Employees.Add(employee);

            if (original is not null)
            {
                TicketTitle = original.Title;
                Description = original.Description;
                SelectedCategory = Categories.FirstOrDefault(x => x.Id == original.CategoryId);
                SelectedDepartment = Departments.FirstOrDefault(x => x.Id == original.AssignedDepartmentId);
                SelectedEmployee = Employees.FirstOrDefault(x => x.Id == original.AssignedEmployeeId);
                Deadline = original.Deadline;
                SelectedStatus = original.Status;
                ApplicantName = original.ApplicantName;
                ApplicantContact = original.ApplicantContact ?? string.Empty;
                ApplicantAddress = original.ApplicantAddress ?? string.Empty;
                foreach (var item in await ticketService.GetHistoryAsync(original.Id)) History.Add(item);
            }
            else
            {
                SelectedDepartment = Departments.FirstOrDefault(x => x.Id == AppSession.DepartmentId);
                Deadline = DateTime.Today.AddDays(3);
            }
            FilterEmployees();
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(TicketTitle) || string.IsNullOrWhiteSpace(Description) || SelectedCategory is null || string.IsNullOrWhiteSpace(ApplicantName))
        {
            NotifyError("Заполните обязательные поля");
            return;
        }
        if (Deadline.HasValue && Deadline.Value < DateTime.Now)
        {
            NotifyError("Дедлайн не может быть в прошлом");
            return;
        }

        try
        {
            var ticket = ToTicket();
            if (IsNewTicket) await ticketService.CreateTicketAsync(ticket);
            else await ticketService.UpdateTicketAsync(original!, ticket, SaveComment);
            DialogHost.Close("RootDialog", true);
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
    }

    [RelayCommand]
    private async Task RejectAsync()
    {
        if (!CanManage) return;
        if (System.Windows.MessageBox.Show("Отклонить заявку?", "Подтверждение", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
        {
            SelectedStatus = "rejected";
            await SaveAsync();
        }
    }

    [RelayCommand]
    private async Task AcceptAsync()
    {
        if (original is null) return;

        IsBusy = true;
        try
        {
            await ticketService.AcceptTicketAsync(original);
            DialogHost.Close("RootDialog", true);
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void Close() => DialogHost.Close("RootDialog", false);

    private Ticket ToTicket() => new()
    {
        Id = original?.Id ?? 0,
        Title = TicketTitle.Trim(),
        Description = Description.Trim(),
        CategoryId = SelectedCategory?.Id,
        Status = SelectedStatus,
        ApplicantName = ApplicantName.Trim(),
        ApplicantContact = ApplicantContact,
        ApplicantAddress = ApplicantAddress,
        AssignedDepartmentId = SelectedDepartment?.Id,
        AssignedEmployeeId = SelectedEmployee?.Id,
        CreatedById = original?.CreatedById,
        CreatedAt = original?.CreatedAt,
        Deadline = Deadline
    };

    private void FilterEmployees()
    {
        AvailableEmployees.Clear();
        foreach (var employee in Employees.Where(x => SelectedDepartment is null || x.DepartmentId == SelectedDepartment.Id)) AvailableEmployees.Add(employee);
        if (SelectedEmployee is not null && !AvailableEmployees.Any(x => x.Id == SelectedEmployee.Id)) SelectedEmployee = null;
    }
}
