using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AdminTaskSystem.Models;

[Table("tasks")]
public class TaskItem : BaseModel
{
    [PrimaryKey("id", false)] public int Id { get; set; }
    [Column("ticket_id")] public int? TicketId { get; set; }
    [Column("title")] public string Title { get; set; } = string.Empty;
    [Column("description")] public string? Description { get; set; }
    [Column("status")] public string Status { get; set; } = "new";
    [Column("assigned_employee_id")] public int? AssignedEmployeeId { get; set; }
    [Column("created_by_id")] public int? CreatedById { get; set; }
    [Column("deadline")] public DateTime? Deadline { get; set; }
    [Column("created_at")] public DateTime? CreatedAt { get; set; }
    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
    [Column("completed_at")] public DateTime? CompletedAt { get; set; }
}
