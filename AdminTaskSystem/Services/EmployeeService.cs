using AdminTaskSystem.Models;

namespace AdminTaskSystem.Services;

public sealed class EmployeeService
{
    public async Task<List<Employee>> GetAllAsync()
    {
        List<Employee> employees;
        if (CacheService.IsOnline)
        {
            var response = await SupabaseService.Client.From<Employee>().Get();
            employees = response.Models.ToList();
            await CacheService.SaveAsync("employees.json", employees);
        }
        else
        {
            employees = await CacheService.LoadAsync<Employee>("employees.json");
        }

        var departments = await GetDepartmentsAsync();
        var map = departments.ToDictionary(x => x.Id, x => x.Name);
        foreach (var employee in employees)
        {
            if (employee.DepartmentId is int id && map.TryGetValue(id, out var name))
            {
                employee.DepartmentName = name;
            }
        }

        return employees.OrderBy(x => x.FullName).ToList();
    }

    public async Task<List<Department>> GetDepartmentsAsync()
    {
        if (CacheService.IsOnline)
        {
            var response = await SupabaseService.Client.From<Department>().Get();
            var data = response.Models.OrderBy(x => x.Name).ToList();
            await CacheService.SaveAsync("departments.json", data);
            return data;
        }

        return (await CacheService.LoadAsync<Department>("departments.json")).OrderBy(x => x.Name).ToList();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        if (CacheService.IsOnline)
        {
            var response = await SupabaseService.Client.From<Category>().Get();
            var data = response.Models.OrderBy(x => x.Name).ToList();
            await CacheService.SaveAsync("categories.json", data);
            return data;
        }

        return (await CacheService.LoadAsync<Category>("categories.json")).OrderBy(x => x.Name).ToList();
    }

    public async Task CreateAsync(Employee employee)
    {
        EnsureOnline();
        employee.IsActive = true;
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;
        await SupabaseService.Client.From<Employee>().Insert(employee);
        await RefreshEmployeeCacheAsync();
    }

    public async Task UpdateAsync(Employee employee)
    {
        EnsureOnline();
        employee.UpdatedAt = DateTime.UtcNow;
        await SupabaseService.Client.From<Employee>().Update(employee);
        await RefreshEmployeeCacheAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        EnsureOnline();
        var employees = await GetAllAsync();
        var employee = employees.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("Сотрудник не найден.");
        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;
        await SupabaseService.Client.From<Employee>().Update(employee);
        await RefreshEmployeeCacheAsync();
    }

    private static void EnsureOnline()
    {
        if (!CacheService.IsOnline)
        {
            throw new InvalidOperationException("Нет подключения. Изменения недоступны в офлайн-режиме.");
        }
    }

    private async Task RefreshEmployeeCacheAsync()
    {
        var response = await SupabaseService.Client.From<Employee>().Get();
        await CacheService.SaveAsync("employees.json", response.Models.ToList());
    }
}
