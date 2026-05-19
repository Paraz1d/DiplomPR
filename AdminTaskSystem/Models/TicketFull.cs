using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;

namespace AdminTaskSystem.Models;

[Table("v_tickets_full")]
public class TicketFull : Ticket
{
    [Column("category")] public string? CategoryName { get; set; }
    [Column("department")] public string? DepartmentName { get; set; }
    [Column("assigned_employee")] public string? AssignedEmployeeName { get; set; }
    [Column("created_by")] public string? CreatedByName { get; set; }
    [Column("deadline_flag")] public string? DeadlineFlag { get; set; }

    [JsonIgnore]
    public bool IsLocked => AssignedEmployeeId.HasValue &&
                            AssignedEmployeeId != AppSession.CurrentUser?.Id &&
                            AppSession.IsEmployee;

    [JsonIgnore]
    public bool IsMine => AssignedEmployeeId == AppSession.CurrentUser?.Id;

    [JsonIgnore]
    public bool CanAccept =>
        AppSession.IsEmployee &&
        !AssignedEmployeeId.HasValue &&
        AssignedDepartmentId == AppSession.DepartmentId &&
        Status is "new" or "in_progress";
}
