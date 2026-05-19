using System.Collections.ObjectModel;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Task = System.Threading.Tasks.Task;

namespace AdminTaskSystem.ViewModels;

public partial class ReportsViewModel : BaseViewModel
{
    private readonly ReportService reportService = new(App.SupabaseService);

    public ObservableCollection<DepartmentStats> DepartmentStats { get; } = [];
    public ObservableCollection<EmployeeStats> EmployeeStats { get; } = [];

    [ObservableProperty]
    private DateTime? dateFrom = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    private DateTime? dateTo = DateTime.Today;

    [ObservableProperty]
    private int totalTickets;

    [ObservableProperty]
    private int doneTickets;

    [ObservableProperty]
    private int overdueTickets;

    [ObservableProperty]
    private double avgCloseHours;

    public ReportsViewModel()
    {
        Title = "Отчеты";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            DepartmentStats.Clear();
            foreach (var item in await reportService.GetDepartmentStatsAsync(DateFrom, DateTo))
            {
                DepartmentStats.Add(item);
            }

            EmployeeStats.Clear();
            foreach (var item in await reportService.GetEmployeeStatsAsync(DateFrom, DateTo))
            {
                EmployeeStats.Add(item);
            }

            TotalTickets = DepartmentStats.Sum(x => x.Total);
            DoneTickets = DepartmentStats.Sum(x => x.DoneCount);
            OverdueTickets = DepartmentStats.Sum(x => x.OverdueCount);
            AvgCloseHours = DepartmentStats.Where(x => x.AvgCloseHours.HasValue).DefaultIfEmpty().Average(x => x?.AvgCloseHours ?? 0);
        }
        catch (Exception ex)
        {
            Notify($"Не удалось загрузить отчеты: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
