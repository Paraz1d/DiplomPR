using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AdminTaskSystem.Models;

[Table("departments")]
public class Department : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("head_employee_id")]
    public int? HeadEmployeeId { get; set; }
}
