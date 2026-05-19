using System.Collections.ObjectModel;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using AdminTaskSystem.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Task = System.Threading.Tasks.Task;

namespace AdminTaskSystem.ViewModels;

public partial class EmployeesViewModel : BaseViewModel
{
    private readonly EmployeeService employeeService = new(App.SupabaseService);

    public ObservableCollection<Employee> Employees { get; } = [];
    public ObservableCollection<Department> Departments { get; } = [];
    public IReadOnlyList<string> Roles { get; } = ["admin", "employee"];

    [ObservableProperty]
    private Employee? selectedEmployee;

    [ObservableProperty]
    private Employee editEmployee = new();

    public EmployeesViewModel()
    {
        Title = "Сотрудники";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            Departments.Clear();
            foreach (var department in await employeeService.GetDepartmentsAsync())
            {
                Departments.Add(department);
            }

            Employees.Clear();
            foreach (var employee in await employeeService.GetEmployeesAsync())
            {
                Employees.Add(employee);
            }
        }
        catch (Exception ex)
        {
            Notify($"Не удалось загрузить сотрудников: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        EditEmployee = new Employee { IsActive = true, Role = "employee" };
        await DialogHost.Show(new EmployeeEditDialog { DataContext = this }, "RootDialog");
        await LoadAsync();
    }

    [RelayCommand]
    private async Task EditAsync(Employee employee)
    {
        EditEmployee = new Employee
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Phone = employee.Phone,
            DepartmentId = employee.DepartmentId,
            Role = employee.Role,
            Password = employee.Password,
            IsActive = employee.IsActive
        };
        await DialogHost.Show(new EmployeeEditDialog { DataContext = this }, "RootDialog");
        await LoadAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditEmployee.FullName) ||
            string.IsNullOrWhiteSpace(EditEmployee.Email) ||
            string.IsNullOrWhiteSpace(EditEmployee.Password))
        {
            Notify("Заполните ФИО, email и пароль.");
            return;
        }

        await employeeService.SaveAsync(EditEmployee);
        DialogHost.Close("RootDialog", true);
        Notify("Сотрудник сохранен.");
    }

    [RelayCommand]
    private async Task DeactivateAsync(Employee employee)
    {
        await employeeService.DeactivateAsync(employee);
        await LoadAsync();
        Notify("Сотрудник деактивирован.");
    }

    [RelayCommand]
    private void CancelEdit() => DialogHost.Close("RootDialog", false);
}
