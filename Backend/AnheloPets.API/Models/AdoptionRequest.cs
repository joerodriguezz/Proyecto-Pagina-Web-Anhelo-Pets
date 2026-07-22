using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

[Table("adoption_requests")]
public class AdoptionRequest
{
    [Key]
    [Column("adoption_request_id")]
    public string? AdoptionRequestId { get; set; }

    [Column("user_id")]
    [ForeignKey("User")]
    public string UserId { get; set; } = string.Empty;

    public User? User { get; set; }

    [Column("animal_id")]
    [ForeignKey("Animal")]
    public string AnimalId { get; set; } = string.Empty;

    public Animal? Animal { get; set; }

    [Column("applicant_name")]
    public string ApplicantName { get; set; } = string.Empty;

    [Column("national_id")]
    public string NationalId { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [Column("age")]
    public int Age { get; set; }

    [Column("has_whatsapp")]
    public bool HasWhatsapp { get; set; }

    [Column("lives_in_costa_rica")]
    public bool LivesInCostaRica { get; set; } = true;

    [Column("foreign_country")]
    public string? ForeignCountry { get; set; }

    [Column("address")]
    public string Address { get; set; } = string.Empty;

    [Column("pet_name_snapshot")]
    public string PetNameSnapshot { get; set; } = string.Empty;

    [Column("reason_for_pet")]
    public string? ReasonForPet { get; set; }

    [Column("adoption_reasons")]
    public string AdoptionReasons { get; set; } = string.Empty;

    [Column("household_members")]
    public string HouseholdMembers { get; set; } = string.Empty;

    [Column("other_pets")]
    public string? OtherPets { get; set; }

    [Column("profession")]
    public string Profession { get; set; } = string.Empty;

    [Column("daily_routine")]
    public string DailyRoutine { get; set; } = string.Empty;

    [Column("hours_alone")]
    public string HoursAlone { get; set; } = string.Empty;

    /// <summary>Pendiente | En proceso | Aprobada | Rechazada (ck_adoption_requests_validation_status).</summary>
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
