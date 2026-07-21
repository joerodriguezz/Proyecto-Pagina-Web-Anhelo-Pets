using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

[Table("animal_medical_records")]
public class AnimalMedicalRecord
{
    [Key]
    [Column("animal_medical_record_id")]
    public long AnimalMedicalRecordId { get; set; }

    [Column("animal_id")]
    public string AnimalId { get; set; } = string.Empty;

    [Column("veterinarian_id")]
    [ForeignKey("Veterinarian")]
    public string VeterinarianId { get; set; } = string.Empty;

    public Veterinarian? Veterinarian { get; set; }

    [Column("diagnosis")]
    public string Diagnosis { get; set; } = string.Empty;

    [Column("treatment")]
    public string Treatment { get; set; } = string.Empty;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("visit_date")]
    public DateOnly VisitDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
}
