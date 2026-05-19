using AdminTaskSystem.Models;
using Task = System.Threading.Tasks.Task;

namespace AdminTaskSystem.Services;

public sealed class AuthService(SupabaseService supabase)
{
    public async Task<Employee?> LoginAsync(string email, string password)
    {
        var response = await supabase.Client
            .From<Employee>()
            .Where(x => x.Email == email)
            .Where(x => x.IsActive == true)
            .Get();

        var employee = response.Models.FirstOrDefault();
        if (employee is null || employee.Password != password)
        {
            return null;
        }

        return employee;
    }
}
