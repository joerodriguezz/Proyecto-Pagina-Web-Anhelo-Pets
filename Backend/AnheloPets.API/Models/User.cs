using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("user_id")]
    public string? UserId { get; set; }

    [Column("username")]
    public required string Username { get; set; } = string.Empty;

    [Column("password_hash")]
    public required string PasswordHash { get; set; } = string.Empty;

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("created_by")]
    public string? CreatedBy { get; set; }
    
    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }
    
    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
}
