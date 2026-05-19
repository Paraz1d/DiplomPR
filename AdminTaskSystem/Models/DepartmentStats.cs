using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AdminTaskSystem.Models;

[Table("v_department_stats")]
public class DepartmentStats : BaseModel
{
    [PrimaryKey("id", false)] public int Id { get; set; }
    [Column("department")] public string Department { get; set; } = string.Empty;
    [Column("total")] public int Total { get; set; }
    [Column("new_count")] public int NewCount { get; set; }
    [Column("in_progress_count")] public int InProgressCount { get; set; }
    [Column("done_count")] public int DoneCount { get; set; }
    [Column("overdue_count")] public int OverdueCount { get; set; }
    [Column("avg_close_hours")] public double? AvgCloseHours { get; set; }
}
