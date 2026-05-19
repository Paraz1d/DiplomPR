using System.Collections.ObjectModel;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using AdminTaskSystem.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace AdminTaskSystem.ViewModels;

public partial class EmployeesViewModel : BaseViewModel
{
    private readonly EmployeeService employeeService = new();
    private List<Employee> all = [];

    [ObservableProperty] private ObservableCollection<Employee> employees = new();
    [ObservableProperty] private string searchText = string.Empty;

    public EmployeesViewModel() { Title = "Сотрудники"; }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            all = await employeeService.GetAllAsync();
            ApplyFilter();
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task CreateEmployeeAsync()
    {
        await DialogHost.Show(new EmployeeEditDialog { DataContext = new EmployeeEditViewModel(null) }, "RootDialog");
        await LoadAsync();
    }

    [RelayCommand]
    private async Task EditEmployeeAsync(Employee employee)
    {
        await DialogHost.Show(new EmployeeEditDialog { DataContext = new EmployeeEditViewModel(employee) }, "RootDialog");
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeactivateEmployeeAsync(Employee employee)
    {
        if (System.Windows.MessageBox.Show("Деактивировать сотрудника?", "Подтверждение", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) != System.Windows.MessageBoxResult.Yes) return;
        try
        {
            await employeeService.DeactivateAsync(employee.Id);
            await LoadAsync();
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
    }

    private void ApplyFilter()
    {
        var query = all.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(SearchText)) query = query.Where(x => x.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || x.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        Employees = new ObservableCollection<Employee>(query);
    }
}
