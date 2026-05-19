using Supabase.Postgrest.Attributes;

namespace AdminTaskSystem.Models;

[Table("v_tasks_full")]
public class TaskFull : TaskItem
{
    [Column("ticket_title")] public string? TicketTitle { get; set; }
    [Column("assigned_employee")] public string? AssignedEmployeeName { get; set; }
    [Column("deadline_flag")] public string? DeadlineFlag { get; set; }
}
