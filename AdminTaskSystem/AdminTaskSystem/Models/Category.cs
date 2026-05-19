using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AdminTaskSystem.Models;

[Table("categories")]
public class Category : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }
}
