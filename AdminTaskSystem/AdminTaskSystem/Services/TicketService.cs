using AdminTaskSystem.Models;
using Task = System.Threading.Tasks.Task;

namespace AdminTaskSystem.Services;

public sealed class TicketService(SupabaseService supabase)
{
    public async Task<IReadOnlyList<TicketFull>> GetTicketsAsync(Employee currentUser)
    {
        var response = await supabase.Client.From<TicketFull>().Get();
        var tickets = response.Models.AsEnumerable();

        if (!currentUser.IsAdmin)
        {
            tickets = tickets.Where(x =>
                x.AssignedEmployeeId == currentUser.Id ||
                (x.AssignedEmployeeId is null && x.AssignedDepartmentId == currentUser.DepartmentId));
        }

        return tickets.OrderByDescending(x => x.CreatedAt).ToList();
    }

    public async Task<Ticket?> GetTicketAsync(int id)
    {
        var response = await supabase.Client.From<Ticket>().Where(x => x.Id == id).Get();
        return response.Models.FirstOrDefault();
    }

    public async Task<IReadOnlyList<TicketHistory>> GetHistoryAsync(int ticketId)
    {
        var response = await supabase.Client.From<TicketHistory>().Where(x => x.TicketId == ticketId).Get();
        var employees = await supabase.Client.From<Employee>().Get();
        var employeeNames = employees.Models.ToDictionary(x => x.Id, x => x.FullName);
        foreach (var item in response.Models)
        {
            if (item.ChangedById is int changedById && employeeNames.TryGetValue(changedById, out var authorName))
            {
                item.AuthorName = authorName;
            }
        }

        return response.Models.OrderByDescending(x => x.ChangedAt).ToList();
    }

    public async Task<Ticket> CreateAsync(Ticket ticket)
    {
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;
        var response = await supabase.Client.From<Ticket>().Insert(ticket);
        return response.Models.FirstOrDefault() ?? ticket;
    }

    public async Task<Ticket> UpdateAsync(Ticket ticket, string? oldStatus, int changedById, string? comment)
    {
        ticket.UpdatedAt = DateTime.UtcNow;
        ticket.ClosedAt = ticket.Status == "done" ? DateTime.UtcNow : null;

        var response = await supabase.Client.From<Ticket>().Update(ticket);

        if (!string.Equals(oldStatus, ticket.Status, StringComparison.OrdinalIgnoreCase))
        {
            await AddHistoryAsync(ticket.Id, changedById, oldStatus, ticket.Status, "status", oldStatus, ticket.Status, comment);
        }

        return response.Models.FirstOrDefault() ?? ticket;
    }

    public async Task DeleteAsync(int id)
    {
        await supabase.Client.From<Ticket>().Where(x => x.Id == id).Delete();
    }

    private async Task AddHistoryAsync(int ticketId, int changedById, string? oldStatus, string? newStatus,
        string fieldChanged, string? oldValue, string? newValue, string? comment)
    {
        var history = new TicketHistory
        {
            TicketId = ticketId,
            ChangedById = changedById,
            ChangedAt = DateTime.UtcNow,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            FieldChanged = fieldChanged,
            OldValue = oldValue,
            NewValue = newValue,
            Comment = comment
        };

        await supabase.Client.From<TicketHistory>().Insert(history);
    }
}
