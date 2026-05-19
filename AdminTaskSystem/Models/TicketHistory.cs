using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AdminTaskSystem.Models;

[Table("ticket_history")]
public class TicketHistory : BaseModel
{
    [PrimaryKey("id", false)] public int Id { get; set; }
    [Column("ticket_id")] public int TicketId { get; set; }
    [Column("changed_by_id")] public int? ChangedById { get; set; }
    [Column("changed_at")] public DateTime? ChangedAt { get; set; }
    [Column("old_status")] public string? OldStatus { get; set; }
    [Column("new_status")] public string? NewStatus { get; set; }
    [Column("field_changed")] public string? FieldChanged { get; set; }
    [Column("old_value")] public string? OldValue { get; set; }
    [Column("new_value")] public string? NewValue { get; set; }
    [Column("comment")] public string? Comment { get; set; }
    [JsonIgnore] public string? ChangedByName { get; set; }
}
