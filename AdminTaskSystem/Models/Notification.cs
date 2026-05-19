using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AdminTaskSystem.Models;

[Table("notifications")]
public class Notification : BaseModel
{
    [PrimaryKey("id", false)] public int Id { get; set; }
    [Column("employee_id")] public int EmployeeId { get; set; }
    [Column("title")] public string Title { get; set; } = string.Empty;
    [Column("body")] public string Body { get; set; } = string.Empty;
    [Column("is_read")] public bool IsRead { get; set; }
    [Column("created_at")] public DateTime? CreatedAt { get; set; }
    [Column("ticket_id")] public int? TicketId { get; set; }
}
