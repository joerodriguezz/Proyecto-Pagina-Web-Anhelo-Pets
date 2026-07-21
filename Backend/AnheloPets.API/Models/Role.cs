using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

[Table("roles")]
public class Role
{
    [Key]
    [Column("role_id")]
    public long RoleId { get; set; }

    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty;

    [Column("role_access")]
    public string RoleAccess { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
}
