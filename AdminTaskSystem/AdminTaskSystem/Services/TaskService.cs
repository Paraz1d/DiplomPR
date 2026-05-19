using AdminTaskSystem.Models;
using DbTask = AdminTaskSystem.Models.Task;

namespace AdminTaskSystem.Services;

public sealed class TaskService(SupabaseService supabase)
{
    public async Task<IReadOnlyList<TaskFull>> GetTasksAsync(Employee currentUser)
    {
        var response = await supabase.Client.From<TaskFull>().Get();
        var tasks = response.Models.AsEnumerable();

        if (!currentUser.IsAdmin)
        {
            tasks = tasks.Where(x => x.AssignedEmployeeId == currentUser.Id);
        }

        return tasks.OrderByDescending(x => x.CreatedAt).ToList();
    }

    public async Task<DbTask?> GetTaskAsync(int id)
    {
        var response = await supabase.Client.From<DbTask>().Where(x => x.Id == id).Get();
        return response.Models.FirstOrDefault();
    }

    public async Task<DbTask> SaveAsync(DbTask task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        task.CompletedAt = task.Status == "done" ? DateTime.UtcNow : null;

        if (task.Id == 0)
        {
            task.CreatedAt = DateTime.UtcNow;
            var insert = await supabase.Client.From<DbTask>().Insert(task);
            return insert.Models.FirstOrDefault() ?? task;
        }

        var update = await supabase.Client.From<DbTask>().Update(task);
        return update.Models.FirstOrDefault() ?? task;
    }
}
