using AdminTaskSystem.Models;

namespace AdminTaskSystem.Services;

public sealed class ReportService
{
    public async Task<List<DepartmentStats>> GetDepartmentStatsAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        if (CacheService.IsOnline)
        {
            var response = await SupabaseService.Client.From<DepartmentStats>().Get();
            var data = response.Models.ToList();
            await CacheService.SaveAsync("department_stats.json", data);
            return data;
        }

        return await CacheService.LoadAsync<DepartmentStats>("department_stats.json");
    }

    public async Task<List<EmployeeStats>> GetEmployeeStatsAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        if (CacheService.IsOnline)
        {
            var response = await SupabaseService.Client.From<EmployeeStats>().Get();
            var data = response.Models.ToList();
            await CacheService.SaveAsync("employee_stats.json", data);
            return data;
        }

        return await CacheService.LoadAsync<EmployeeStats>("employee_stats.json");
    }
}
