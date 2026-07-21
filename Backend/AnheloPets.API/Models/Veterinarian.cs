using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

[Table("veterinarians")]
public class Veterinarian
{
    [Key]
    [Column("veterinarian_id")]
    public string? VeterinarianId { get; set; }

    [Column("volunteer_id")]
    [ForeignKey("Volunteer")]
    public string VolunteerId { get; set; } = string.Empty;

    public Volunteer? Volunteer { get; set; }

    [Column("specialty")]
    public string Specialty { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
}
