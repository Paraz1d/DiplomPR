using Supabase.Postgrest.Attributes;

namespace AdminTaskSystem.Models;

[Table("v_tickets_full")]
public class TicketFull : Ticket
{
    [Column("category_name")]
    public string? CategoryName { get; set; }

    [Column("department_name")]
    public string? DepartmentName { get; set; }

    [Column("assigned_employee_name")]
    public string? AssignedEmployeeName { get; set; }

    [Column("created_by_name")]
    public string? CreatedByName { get; set; }

    [Column("deadline_flag")]
    public string? DeadlineFlag { get; set; }
}
