using System.Collections.ObjectModel;
using System.IO;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace AdminTaskSystem.ViewModels;

public partial class ReportsViewModel : BaseViewModel
{
    private readonly ReportService reportService = new();
    private readonly ExcelExportService excelExportService = new();

    [ObservableProperty] private ObservableCollection<DepartmentStats> deptStats = new();
    [ObservableProperty] private ObservableCollection<EmployeeStats> empStats = new();
    [ObservableProperty] private DateTime? dateFrom = DateTime.Today.AddMonths(-1);
    [ObservableProperty] private DateTime? dateTo = DateTime.Today;
    [ObservableProperty] private int totalTickets;
    [ObservableProperty] private int totalDone;
    [ObservableProperty] private int totalOverdue;
    [ObservableProperty] private double avgCloseHours;

    public bool CanExport => AppSession.CanManage;

    public ReportsViewModel() { Title = "Отчёты"; }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            DeptStats = new ObservableCollection<DepartmentStats>(await reportService.GetDepartmentStatsAsync(DateFrom, DateTo));
            EmpStats = new ObservableCollection<EmployeeStats>(await reportService.GetEmployeeStatsAsync(DateFrom, DateTo));
            TotalTickets = DeptStats.Sum(x => x.Total);
            TotalDone = DeptStats.Sum(x => x.DoneCount);
            TotalOverdue = DeptStats.Sum(x => x.OverdueCount);
            AvgCloseHours = DeptStats.Where(x => x.AvgCloseHours.HasValue).Select(x => x.AvgCloseHours!.Value).DefaultIfEmpty(0).Average();
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportAsync()
    {
        var dialog = new SaveFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", FileName = "reports.xlsx" };
        if (dialog.ShowDialog() == true)
        {
            if (DeptStats.Count == 0 && EmpStats.Count == 0)
            {
                await LoadAsync();
            }

            excelExportService.ExportReports(DeptStats, EmpStats, dialog.FileName);
            Notify("Файл отчёта сохранён");
        }
    }
}
