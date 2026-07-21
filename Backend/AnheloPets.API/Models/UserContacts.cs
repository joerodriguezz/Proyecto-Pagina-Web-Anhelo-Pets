using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

[Table("user_contacts")]
public class UserContacts
{
    [Key]
    [Column("user_contact_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long UserContactId { get; set; }
    
    [Column("user_id")]
    [ForeignKey("User")]
    public string UserId { get; set; } = string.Empty;
    
    public User? User { get; set; }
    
    [Column("email")]
    public required string Email { get; set; }
    
    [Column("phone_primary")]
    public required string PhonePrimary { get; set; }
    
    [Column("phone_secondary")]
    public string? PhoneSecondary { get; set; }
    
    [Column("city")]
    public string? City { get; set; }
    
    [Column("town")]
    public string? Town { get; set; }

    [Column("district")]
    public string? District { get; set; }

    [Column("address_line")]
    public string? AddressLine { get; set; }
    
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [Column("created_by")]
    public string? CreatedBy { get; set; }
    
    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }
    
    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
}