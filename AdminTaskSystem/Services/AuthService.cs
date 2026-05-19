using AdminTaskSystem.Models;

namespace AdminTaskSystem.Services;

public sealed class AuthService
{
    public async Task<Employee?> LoginAsync(string email, string password)
    {
        var normalizedEmail = email.Trim();
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

        var employee = employees.FirstOrDefault(x =>
            x.IsActive &&
            string.Equals(x.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
            x.Password == password);

        if (employee is not null)
        {
            await CacheService.SaveAsync("current_user.json", new List<Employee> { employee });
        }

        return employee;
    }
}
