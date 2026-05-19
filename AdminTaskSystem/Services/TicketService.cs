using AdminTaskSystem.Models;

namespace AdminTaskSystem.Services;

public sealed class TicketService
{
    public async Task<List<TicketFull>> GetTicketsAsync()
    {
        List<TicketFull> tickets;
        if (CacheService.IsOnline)
        {
            try
            {
                var response = await SupabaseService.Client.From<TicketFull>().Get();
                tickets = response.Models.ToList();
                if (ShouldRebuildFromTables(tickets))
                {
                    tickets = await BuildTicketsFromTablesAsync();
                }
            }
            catch
            {
                tickets = await BuildTicketsFromTablesAsync();
            }

            await CacheService.SaveAsync("tickets.json", tickets);
        }
        else
        {
            tickets = await CacheService.LoadAsync<TicketFull>("tickets.json");
        }

        if (!AppSession.IsAdmin)
        {
            var userId = AppSession.CurrentUser?.Id;
            tickets = AppSession.DepartmentId is int departmentId
                ? tickets.Where(x =>
                    x.AssignedDepartmentId == departmentId ||
                    x.AssignedEmployeeId == userId ||
                    x.CreatedById == userId).ToList()
                : tickets.Where(x =>
                    x.AssignedEmployeeId == userId ||
                    x.CreatedById == userId).ToList();
        }

        return tickets.OrderByDescending(x => x.CreatedAt).ToList();
    }

    public async Task<List<TicketHistory>> GetHistoryAsync(int ticketId)
    {
        List<TicketHistory> history;
        if (CacheService.IsOnline)
        {
            var response = await SupabaseService.Client.From<TicketHistory>().Get();
            history = response.Models.Where(x => x.TicketId == ticketId).ToList();
        }
        else
        {
            history = (await CacheService.LoadAsync<TicketHistory>("ticket_history.json")).Where(x => x.TicketId == ticketId).ToList();
        }

        var employees = await CacheService.LoadAsync<Employee>("employees.json");
        if (employees.Count == 0 && CacheService.IsOnline)
        {
            employees = (await SupabaseService.Client.From<Employee>().Get()).Models.ToList();
            await CacheService.SaveAsync("employees.json", employees);
        }

        var names = employees.ToDictionary(x => x.Id, x => x.FullName);
        foreach (var item in history)
        {
            if (item.ChangedById is int id && names.TryGetValue(id, out var name))
            {
                item.ChangedByName = name;
            }
        }

        return history.OrderByDescending(x => x.ChangedAt).ToList();
    }

    public async Task CreateTicketAsync(Ticket ticket)
    {
        EnsureOnline();
        ticket.CreatedById = AppSession.CurrentUser?.Id;
        ticket.Status = "new";
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;
        var created = (await SupabaseService.Client.From<Ticket>().Insert(ticket)).Models.FirstOrDefault() ?? ticket;
        await SupabaseService.Client.From<TicketHistory>().Insert(new TicketHistory
        {
            TicketId = created.Id,
            ChangedById = AppSession.CurrentUser?.Id,
            ChangedAt = DateTime.UtcNow,
            NewStatus = "new",
            FieldChanged = "status",
            NewValue = "new",
            Comment = "Создана заявка"
        });
        await RefreshTicketCacheAsync();
    }

    public async Task UpdateTicketAsync(TicketFull old, Ticket updated, string? comment)
    {
        EnsureOnline();
        updated.UpdatedAt = DateTime.UtcNow;
        updated.ClosedAt = updated.Status is "done" or "rejected" ? DateTime.UtcNow : null;
        await SupabaseService.Client.From<Ticket>().Update(updated);
        if (!string.Equals(old.Status, updated.Status, StringComparison.OrdinalIgnoreCase))
        {
            await SupabaseService.Client.From<TicketHistory>().Insert(new TicketHistory
            {
                TicketId = updated.Id,
                ChangedById = AppSession.CurrentUser?.Id,
                ChangedAt = DateTime.UtcNow,
                OldStatus = old.Status,
                NewStatus = updated.Status,
                FieldChanged = "status",
                OldValue = old.Status,
                NewValue = updated.Status,
                Comment = comment
            });
        }
        await RefreshTicketCacheAsync();
    }

    public async Task AcceptTicketAsync(TicketFull ticket)
    {
        EnsureOnline();

        if (!AppSession.IsEmployee || AppSession.CurrentUser is null)
        {
            throw new InvalidOperationException("Принять заявку может только сотрудник.");
        }

        if (ticket.AssignedEmployeeId.HasValue)
        {
            throw new InvalidOperationException("Заявка уже назначена исполнителю.");
        }

        if (ticket.AssignedDepartmentId != AppSession.DepartmentId)
        {
            throw new InvalidOperationException("Заявка относится к другому отделу.");
        }

        var updated = new Ticket
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            CategoryId = ticket.CategoryId,
            Status = "in_progress",
            ApplicantName = ticket.ApplicantName,
            ApplicantContact = ticket.ApplicantContact,
            ApplicantAddress = ticket.ApplicantAddress,
            AssignedDepartmentId = ticket.AssignedDepartmentId,
            AssignedEmployeeId = AppSession.CurrentUser.Id,
            CreatedById = ticket.CreatedById,
            Deadline = ticket.Deadline,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            ClosedAt = ticket.ClosedAt
        };

        await SupabaseService.Client.From<Ticket>().Update(updated);
        await SupabaseService.Client.From<TicketHistory>().Insert(new TicketHistory
        {
            TicketId = ticket.Id,
            ChangedById = AppSession.CurrentUser.Id,
            ChangedAt = DateTime.UtcNow,
            OldStatus = ticket.Status,
            NewStatus = "in_progress",
            FieldChanged = "assigned_employee_id",
            OldValue = string.Empty,
            NewValue = AppSession.CurrentUser.FullName,
            Comment = "Заявка принята сотрудником в работу"
        });
        await RefreshTicketCacheAsync();
    }

    public static bool CanEditTicket(TicketFull ticket) =>
        AppSession.CanManage || (AppSession.IsEmployee && ticket.AssignedEmployeeId == AppSession.CurrentUser?.Id);

    private static void EnsureOnline()
    {
        if (!CacheService.IsOnline)
        {
            throw new InvalidOperationException("Нет подключения. Изменения недоступны в офлайн-режиме.");
        }
    }

    private async Task RefreshTicketCacheAsync()
    {
        try
        {
            var response = await SupabaseService.Client.From<TicketFull>().Get();
            var tickets = response.Models.ToList();
            if (ShouldRebuildFromTables(tickets))
            {
                tickets = await BuildTicketsFromTablesAsync();
            }

            await CacheService.SaveAsync("tickets.json", tickets);
        }
        catch
        {
            await CacheService.SaveAsync("tickets.json", await BuildTicketsFromTablesAsync());
        }
    }

    private static bool ShouldRebuildFromTables(List<TicketFull> tickets)
    {
        if (tickets.Count == 0)
        {
            return true;
        }

        var missingDepartmentIds = tickets.Count(x =>
            !x.AssignedDepartmentId.HasValue &&
            !string.IsNullOrWhiteSpace(x.DepartmentName));

        return missingDepartmentIds > 0;
    }

    private static async Task<List<TicketFull>> BuildTicketsFromTablesAsync()
    {
        var ticketResponse = await SupabaseService.Client.From<Ticket>().Get();
        var categoryResponse = await SupabaseService.Client.From<Category>().Get();
        var departmentResponse = await SupabaseService.Client.From<Department>().Get();
        var employeeResponse = await SupabaseService.Client.From<Employee>().Get();

        var categories = categoryResponse.Models.ToDictionary(x => x.Id, x => x.Name);
        var departments = departmentResponse.Models.ToDictionary(x => x.Id, x => x.Name);
        var employees = employeeResponse.Models.ToDictionary(x => x.Id, x => x.FullName);

        await CacheService.SaveAsync("categories.json", categoryResponse.Models.ToList());
        await CacheService.SaveAsync("departments.json", departmentResponse.Models.ToList());
        await CacheService.SaveAsync("employees.json", employeeResponse.Models.ToList());

        return ticketResponse.Models.Select(ticket =>
        {
            categories.TryGetValue(ticket.CategoryId ?? 0, out var categoryName);
            departments.TryGetValue(ticket.AssignedDepartmentId ?? 0, out var departmentName);
            employees.TryGetValue(ticket.AssignedEmployeeId ?? 0, out var assignedName);
            employees.TryGetValue(ticket.CreatedById ?? 0, out var createdByName);

            return new TicketFull
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                CategoryId = ticket.CategoryId,
                Status = ticket.Status,
                ApplicantName = ticket.ApplicantName,
                ApplicantContact = ticket.ApplicantContact,
                ApplicantAddress = ticket.ApplicantAddress,
                AssignedDepartmentId = ticket.AssignedDepartmentId,
                AssignedEmployeeId = ticket.AssignedEmployeeId,
                CreatedById = ticket.CreatedById,
                Deadline = ticket.Deadline,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                ClosedAt = ticket.ClosedAt,
                CategoryName = categoryName,
                DepartmentName = departmentName,
                AssignedEmployeeName = assignedName,
                CreatedByName = createdByName,
                DeadlineFlag = GetDeadlineFlag(ticket)
            };
        }).ToList();
    }

    private static string GetDeadlineFlag(Ticket ticket)
    {
        if (ticket.Deadline is not DateTime deadline ||
            ticket.Status is "done" or "rejected" or "overdue")
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
