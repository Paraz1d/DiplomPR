using System.Collections.ObjectModel;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using AdminTaskSystem.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Task = System.Threading.Tasks.Task;

namespace AdminTaskSystem.ViewModels;

public partial class TicketsViewModel : BaseViewModel
{
    private readonly TicketService ticketService = new(App.SupabaseService);
    private readonly EmployeeService employeeService = new(App.SupabaseService);
    private List<TicketFull> allTickets = [];

    public ObservableCollection<TicketFull> Tickets { get; } = [];
    public ObservableCollection<Department> Departments { get; } = [];
    public IReadOnlyList<string> Statuses { get; } = ["Все статусы", "new", "in_progress", "done", "overdue", "rejected"];
    public bool IsAdmin => App.CurrentUser?.IsAdmin == true;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedStatus = "Все статусы";

    [ObservableProperty]
    private Department? selectedDepartment;

    public TicketsViewModel()
    {
        Title = "Заявки";
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedStatusChanged(string value) => ApplyFilters();
    partial void OnSelectedDepartmentChanged(Department? value) => ApplyFilters();

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
            Departments.Clear();
            var departments = await employeeService.GetDepartmentsAsync();
            foreach (var department in departments.Where(d => IsAdmin || d.Id == App.CurrentUser.DepartmentId))
            {
                Departments.Add(department);
            }

            allTickets = (await ticketService.GetTicketsAsync(App.CurrentUser)).ToList();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            Notify($"Не удалось загрузить заявки: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenTicketAsync(TicketFull ticket)
    {
        var detail = new TicketDetailViewModel(ticket.Id);
        var view = new TicketDetailView { DataContext = detail };
        await DialogHost.Show(view, "RootDialog");
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NewTicketAsync()
    {
        if (!IsAdmin)
        {
            return;
        }

        var detail = new TicketDetailViewModel(null);
        var view = new TicketDetailView { DataContext = detail };
        await DialogHost.Show(view, "RootDialog");
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteTicketAsync(TicketFull ticket)
    {
        if (!IsAdmin)
        {
            return;
        }

        await ticketService.DeleteAsync(ticket.Id);
        await LoadAsync();
        Notify("Заявка удалена.");
    }

    private void ApplyFilters()
    {
        Tickets.Clear();
        var query = allTickets.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim();
            query = query.Where(x =>
                x.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.ApplicantName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SelectedStatus) && SelectedStatus != "Все статусы")
        {
            query = query.Where(x => x.Status == SelectedStatus);
        }

        if (SelectedDepartment is not null)
        {
            query = query.Where(x => x.AssignedDepartmentId == SelectedDepartment.Id);
        }

        foreach (var ticket in query)
        {
            Tickets.Add(ticket);
        }
    }
}
