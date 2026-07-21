using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

[Table("user_roles")]
public class UserRole
{
    [Key]
    [Column("user_role_id")]
    public long UserRoleId { get; set; }

    [Column("user_id")]
    [ForeignKey("User")]
    public string UserId { get; set; } = string.Empty;

    public User? User { get; set; }

    [Column("role_id")]
    [ForeignKey("Role")]
    public long RoleId { get; set; }

    public Role? Role { get; set; }

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
