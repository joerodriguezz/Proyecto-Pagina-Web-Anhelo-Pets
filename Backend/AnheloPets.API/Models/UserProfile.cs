using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

[Table("user_profiles")]
public class UserProfile
{
    [Key]
    [Column("user_profile_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long UserProfileId { get; set; }
    
    [Column("user_id")]
    [ForeignKey("User")]
    public string UserId { get; set; } = string.Empty;
    
    public User? User { get; set; }
    
    [Column("national_id")]
    public required string NationalityId { get; set; }
    
    [Column("first_name")]
    public required string FirstName { get; set; }
    
    [Column("middle_name")]
    public string? MiddleName { get; set; }
    
    [Column("last_name")]
    public required string LastName { get; set; }
    
    [Column("second_last_name")]
    public string? SecondLastName { get; set; }
    
    [Column("birth_date")]
    public DateOnly? BirthDate { get; set; }
    
    [Column("nationality")]

    public required string Nationality { get; set; }

    [Column("photo_url")]
    public string? PhotoUrl { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [Column("created_by")]
    public string? CreatedBy { get; set; }
    
    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }
    
    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
}