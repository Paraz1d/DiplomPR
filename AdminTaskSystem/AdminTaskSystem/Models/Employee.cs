using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AdminTaskSystem.Models;

[Table("employees")]
public class Employee : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("department_id")]
    public int? DepartmentId { get; set; }

    [Column("role")]
    public string Role { get; set; } = "employee";

    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    public string? DepartmentName { get; set; }

    public bool IsAdmin => string.Equals(Role, "admin", StringComparison.OrdinalIgnoreCase);
}
