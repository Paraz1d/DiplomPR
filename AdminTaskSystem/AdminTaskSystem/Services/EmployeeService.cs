using AdminTaskSystem.Models;
using Task = System.Threading.Tasks.Task;

namespace AdminTaskSystem.Services;

public sealed class EmployeeService(SupabaseService supabase)
{
    public async Task<IReadOnlyList<Employee>> GetEmployeesAsync()
    {
        var response = await supabase.Client.From<Employee>().Get();
        var departments = (await GetDepartmentsAsync()).ToDictionary(x => x.Id, x => x.Name);
        foreach (var employee in response.Models)
        {
            if (employee.DepartmentId is int departmentId && departments.TryGetValue(departmentId, out var departmentName))
            {
                employee.DepartmentName = departmentName;
            }
        }

        return response.Models.OrderBy(x => x.FullName).ToList();
    }

    public async Task<IReadOnlyList<Department>> GetDepartmentsAsync()
    {
        var response = await supabase.Client.From<Department>().Get();
        return response.Models.OrderBy(x => x.Name).ToList();
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync()
    {
        var response = await supabase.Client.From<Category>().Get();
        return response.Models.OrderBy(x => x.Name).ToList();
    }

    public async Task<Employee> SaveAsync(Employee employee)
    {
        if (employee.Id == 0)
        {
            var insert = await supabase.Client.From<Employee>().Insert(employee);
            return insert.Models.FirstOrDefault() ?? employee;
        }

        var update = await supabase.Client.From<Employee>().Update(employee);
        return update.Models.FirstOrDefault() ?? employee;
    }

    public async Task DeactivateAsync(Employee employee)
    {
        employee.IsActive = false;
        await SaveAsync(employee);
    }
}
