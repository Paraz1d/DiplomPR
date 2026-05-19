using System.Collections.ObjectModel;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using AdminTaskSystem.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

namespace AdminTaskSystem.ViewModels;

public partial class TicketsViewModel : BaseViewModel
{
    private readonly TicketService ticketService = new();
    private readonly EmployeeService employeeService = new();
    private readonly ExcelExportService excelExportService = new();

    [ObservableProperty] private ObservableCollection<TicketFull> allTickets = new();
    [ObservableProperty] private ObservableCollection<TicketFull> filteredTickets = new();
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private string selectedStatusFilter = "Все";
    [ObservableProperty] private string selectedCategoryFilter = "Все";
    [ObservableProperty] private bool showOnlyMyTickets;

    public List<string> StatusFilters { get; } = ["Все", "Новая", "В работе", "Выполнена", "Просрочена", "Отклонена"];
    public ObservableCollection<string> CategoryFilters { get; } = ["Все"];
    public int TotalCount => FilteredTickets.Count;
    public int InProgressCount => FilteredTickets.Count(t => t.Status == "in_progress");
    public int DoneCount => FilteredTickets.Count(t => t.Status == "done");
    public int OverdueCount => FilteredTickets.Count(t => t.Status == "overdue");
    public int MyTicketsCount => AllTickets.Count(t => t.AssignedEmployeeId == AppSession.CurrentUser?.Id);
    public bool HasTickets => FilteredTickets.Count > 0;
    public bool CanManage => AppSession.CanManage;
    public bool CanUseFilters => AppSession.IsAdmin;

    public TicketsViewModel() { Title = "Заявки"; }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedStatusFilterChanged(string value) => ApplyFilters();
    partial void OnSelectedCategoryFilterChanged(string value) => ApplyFilters();
    partial void OnShowOnlyMyTicketsChanged(bool value) => ApplyFilters();

    public void ApplyFilters()
    {
        var status = SelectedStatusFilter switch
        {
            "Новая" => "new",
            "В работе" => "in_progress",
            "Выполнена" => "done",
            "Просрочена" => "overdue",
            "Отклонена" => "rejected",
            _ => string.Empty
        };

        var query = AllTickets.AsEnumerable();
        if (CanUseFilters)
        {
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
            if (SelectedCategoryFilter != "Все") query = query.Where(x => x.CategoryName == SelectedCategoryFilter);
            if (ShowOnlyMyTickets) query = query.Where(x => x.AssignedEmployeeId == AppSession.CurrentUser?.Id);
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.Trim();
                query = query.Where(x => x.Title.Contains(s, StringComparison.OrdinalIgnoreCase) || x.ApplicantName.Contains(s, StringComparison.OrdinalIgnoreCase));
            }
        }

        FilteredTickets = new ObservableCollection<TicketFull>(query);
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(InProgressCount));
        OnPropertyChanged(nameof(DoneCount));
        OnPropertyChanged(nameof(OverdueCount));
        OnPropertyChanged(nameof(MyTicketsCount));
        OnPropertyChanged(nameof(HasTickets));
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            AllTickets = new ObservableCollection<TicketFull>(await ticketService.GetTicketsAsync());
            CategoryFilters.Clear();
            CategoryFilters.Add("Все");
            foreach (var item in AllTickets.Select(x => x.CategoryName).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x)) CategoryFilters.Add(item!);
            ApplyFilters();
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task OpenTicketAsync(TicketFull? ticket)
    {
        var vm = new TicketDetailViewModel(ticket);
        await DialogHost.Show(new TicketDetailDialog { DataContext = vm }, "RootDialog");
        await LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanManage))]
    private async Task CreateTicketAsync() => await OpenTicketAsync(null);

    [RelayCommand]
    private async Task AcceptTicketAsync(TicketFull? ticket)
    {
        if (ticket is null) return;

        IsBusy = true;
        try
        {
            await ticketService.AcceptTicketAsync(ticket);
            Notify("Заявка принята в работу");
            await LoadAsync();
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanManage))]
    private async Task ExportToExcelAsync()
    {
        try
        {
            var dialog = new SaveFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", FileName = "tickets.xlsx" };
            if (dialog.ShowDialog() == true)
            {
                excelExportService.ExportTickets(FilteredTickets, dialog.FileName);
                Notify("Экспорт завершён");
            }
        }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
    }
}
