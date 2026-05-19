using System.Collections.ObjectModel;
using AdminTaskSystem.Models;
using AdminTaskSystem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace AdminTaskSystem.ViewModels;

public partial class EmployeeEditViewModel : BaseViewModel
{
    private readonly Employee? original;
    private readonly EmployeeService employeeService = new();

    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private Department? selectedDepartment;
    [ObservableProperty] private string selectedRole = "employee";
    [ObservableProperty] private string password = string.Empty;

    public ObservableCollection<Department> Departments { get; } = [];
    public List<string> Roles { get; } = ["admin", "manager", "employee"];
    public bool IsNewEmployee => original is null;
    public string HeaderTitle => IsNewEmployee ? "Новый сотрудник" : $"Редактирование: {original!.FullName}";

    public EmployeeEditViewModel(Employee? employee)
    {
        original = employee;
        Title = HeaderTitle;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            Departments.Clear();
            foreach (var department in await employeeService.GetDepartmentsAsync()) Departments.Add(department);
            if (original is not null)
            {
                FullName = original.FullName;
                Email = original.Email;
                Phone = original.Phone ?? string.Empty;
                SelectedRole = original.Role;
                SelectedDepartment = Departments.FirstOrDefault(x => x.Id == original.DepartmentId);
            }
            else
            {
                SelectedDepartment = Departments.FirstOrDefault();
                SelectedRole = "employee";
            }
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || SelectedDepartment is null || string.IsNullOrWhiteSpace(SelectedRole) || (IsNewEmployee && string.IsNullOrWhiteSpace(Password)))
        {
            NotifyError("Заполните обязательные поля");
            return;
        }
        try
        {
            var employee = original ?? new Employee();
            employee.FullName = FullName.Trim();
            employee.Email = Email.Trim();
            employee.Phone = Phone;
            employee.DepartmentId = SelectedDepartment.Id;
            employee.Role = SelectedRole;
            if (!string.IsNullOrWhiteSpace(Password)) employee.Password = Password;
            if (IsNewEmployee) await employeeService.CreateAsync(employee); else await employeeService.UpdateAsync(employee);
            DialogHost.Close("RootDialog", true);
        }
        catch (InvalidOperationException ex) { NotifyError(ex.Message); }
        catch (Exception ex) { NotifyError("Ошибка: " + ex.Message); }
    }

    [RelayCommand]
    private void Close() => DialogHost.Close("RootDialog", false);
}
