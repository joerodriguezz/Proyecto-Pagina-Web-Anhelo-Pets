using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

[Table("volunteers")]
public class Volunteer
{
    [Key]
    [Column("volunteer_id")]
    public string? VolunteerId { get; set; }

    [Column("user_id")]
    [ForeignKey("User")]
    public string UserId { get; set; } = string.Empty;

    public User? User { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("national_id")]
    public string? NationalId { get; set; }

    [Column("volunteer_type")]
    public string? VolunteerType { get; set; }

    [Column("motivation")]
    public string? Motivation { get; set; }

    /// <summary>
    /// JSON serializado con las respuestas específicas del tipo de voluntariado elegido
    /// (casa cuna, transporte, veterinaria, etc.). La tabla no modela cada campo por
    /// columna porque cada tipo tiene un cuestionario distinto; se guarda tal cual lo
    /// arma el formulario público.
    /// </summary>
    [Column("application_details")]
    public string? ApplicationDetails { get; set; }

    /// <summary>Pendiente | Aprobado | Rechazado (ck_volunteers_validation_status).</summary>
    [Column("validation_status")]
    public string ValidationStatus { get; set; } = "Pendiente";

    [Column("validation_notes")]
    public string? ValidationNotes { get; set; }

    [Column("validated_at")]
    public DateTime? ValidatedAt { get; set; }

    [Column("validated_by_user_id")]
    public string? ValidatedByUserId { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
}
