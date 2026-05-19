using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AdminTaskSystem.Models;

[Table("tickets")]
public class Ticket : BaseModel
{
    [PrimaryKey("id", false)] public int Id { get; set; }
    [Column("title")] public string Title { get; set; } = string.Empty;
    [Column("description")] public string Description { get; set; } = string.Empty;
    [Column("category_id")] public int? CategoryId { get; set; }
    [Column("status")] public string Status { get; set; } = "new";
    [Column("applicant_name")] public string ApplicantName { get; set; } = string.Empty;
    [Column("applicant_contact")] public string? ApplicantContact { get; set; }
    [Column("applicant_address")] public string? ApplicantAddress { get; set; }
    [Column("assigned_department_id")] public int? AssignedDepartmentId { get; set; }
    [Column("assigned_employee_id")] public int? AssignedEmployeeId { get; set; }
    [Column("created_by_id")] public int? CreatedById { get; set; }
    [Column("deadline")] public DateTime? Deadline { get; set; }
    [Column("created_at")] public DateTime? CreatedAt { get; set; }
    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
    [Column("closed_at")] public DateTime? ClosedAt { get; set; }
}
