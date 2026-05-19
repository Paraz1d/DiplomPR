using AdminTaskSystem.Models;
using Task = System.Threading.Tasks.Task;

namespace AdminTaskSystem.Services;

public sealed class ReportService(SupabaseService supabase)
{
    public async Task<IReadOnlyList<DepartmentStats>> GetDepartmentStatsAsync(DateTime? from, DateTime? to)
    {
        var response = await supabase.Client.From<DepartmentStats>().Get();
        return response.Models.ToList();
    }

    public async Task<IReadOnlyList<EmployeeStats>> GetEmployeeStatsAsync(DateTime? from, DateTime? to)
    {
        var response = await supabase.Client.From<EmployeeStats>().Get();
        return response.Models.ToList();
    }
}
