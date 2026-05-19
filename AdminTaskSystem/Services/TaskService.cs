using AdminTaskSystem.Models;

namespace AdminTaskSystem.Services;

public sealed class TaskService
{
    public async Task<List<TaskFull>> GetTasksAsync()
    {
        List<TaskFull> tasks;
        if (CacheService.IsOnline)
        {
            try
            {
                var response = await SupabaseService.Client.From<TaskFull>().Get();
                tasks = response.Models.ToList();
                if (ShouldRebuildFromTables(tasks))
                {
                    tasks = await BuildTasksFromTablesAsync();
                }
            }
            catch
            {
                tasks = await BuildTasksFromTablesAsync();
            }

            await CacheService.SaveAsync("tasks.json", tasks);
        }
        else
        {
            tasks = await CacheService.LoadAsync<TaskFull>("tasks.json");
        }

        if (AppSession.IsEmployee)
        {
            tasks = tasks.Where(x => x.AssignedEmployeeId == AppSession.CurrentUser?.Id).ToList();
        }

        return tasks.OrderByDescending(x => x.CreatedAt).ToList();
    }

    public async Task CreateTaskAsync(TaskItem task)
    {
        EnsureOnline();
        task.CreatedById = AppSession.CurrentUser?.Id;
        task.Status = string.IsNullOrWhiteSpace(task.Status) ? "new" : task.Status;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        await SupabaseService.Client.From<TaskItem>().Insert(task);
        await RefreshTaskCacheAsync();
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        EnsureOnline();
        task.UpdatedAt = DateTime.UtcNow;
        task.CompletedAt = task.Status == "done" ? DateTime.UtcNow : null;
        await SupabaseService.Client.From<TaskItem>().Update(task);
        await RefreshTaskCacheAsync();
    }

    private static void EnsureOnline()
    {
        if (!CacheService.IsOnline)
        {
            throw new InvalidOperationException("Нет подключения. Изменения недоступны в офлайн-режиме.");
        }
    }

    private async Task RefreshTaskCacheAsync()
    {
        try
        {
            var response = await SupabaseService.Client.From<TaskFull>().Get();
            var tasks = response.Models.ToList();
            if (ShouldRebuildFromTables(tasks))
            {
                tasks = await BuildTasksFromTablesAsync();
            }

            await CacheService.SaveAsync("tasks.json", tasks);
        }
        catch
        {
            await CacheService.SaveAsync("tasks.json", await BuildTasksFromTablesAsync());
        }
    }

    private static bool ShouldRebuildFromTables(List<TaskFull> tasks)
    {
        if (tasks.Count == 0)
        {
            return true;
        }

        var missingAssigneeIds = tasks.Count(x =>
            !x.AssignedEmployeeId.HasValue &&
            !string.IsNullOrWhiteSpace(x.AssignedEmployeeName));

        return missingAssigneeIds > 0;
    }

    private static async Task<List<TaskFull>> BuildTasksFromTablesAsync()
    {
        var taskResponse = await SupabaseService.Client.From<TaskItem>().Get();
        var employeeResponse = await SupabaseService.Client.From<Employee>().Get();
        var ticketResponse = await SupabaseService.Client.From<Ticket>().Get();

        var employees = employeeResponse.Models.ToDictionary(x => x.Id, x => x.FullName);
        var tickets = ticketResponse.Models.ToDictionary(x => x.Id, x => x.Title);

        await CacheService.SaveAsync("employees.json", employeeResponse.Models.ToList());
        await CacheService.SaveAsync("tickets_base.json", ticketResponse.Models.ToList());

        return taskResponse.Models.Select(task =>
        {
            employees.TryGetValue(task.AssignedEmployeeId ?? 0, out var assignedName);
            tickets.TryGetValue(task.TicketId ?? 0, out var ticketTitle);

            return new TaskFull
            {
                Id = task.Id,
                TicketId = task.TicketId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                AssignedEmployeeId = task.AssignedEmployeeId,
                CreatedById = task.CreatedById,
                Deadline = task.Deadline,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                CompletedAt = task.CompletedAt,
                TicketTitle = ticketTitle,
                AssignedEmployeeName = assignedName,
                DeadlineFlag = GetDeadlineFlag(task)
            };
        }).OrderByDescending(x => x.CreatedAt).ToList();
    }

    private static string GetDeadlineFlag(TaskItem task)
    {
        if (task.Deadline is not DateTime deadline ||
            task.Status is "done" or "overdue")
        {
            return string.Empty;
        }

        var now = DateTime.UtcNow;
        if (deadline < now)
        {
            return "overdue";
        }

        return deadline <= now.AddDays(2) ? "warning" : string.Empty;
    }
}
