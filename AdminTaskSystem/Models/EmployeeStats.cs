using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AdminTaskSystem.Models;

[Table("v_employee_stats")]
public class EmployeeStats : BaseModel
{
    [PrimaryKey("id", false)] public int Id { get; set; }
    [Column("full_name")] public string FullName { get; set; } = string.Empty;
    [Column("department")] public string? Department { get; set; }
    [Column("tickets_total")] public int TicketsTotal { get; set; }
    [Column("tickets_done")] public int TicketsDone { get; set; }
    [Column("tickets_overdue")] public int TicketsOverdue { get; set; }
    [Column("tasks_total")] public int TasksTotal { get; set; }
    [Column("tasks_done")] public int TasksDone { get; set; }
}
