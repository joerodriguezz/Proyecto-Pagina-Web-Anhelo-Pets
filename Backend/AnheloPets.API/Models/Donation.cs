using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

[Table("donations")]
public class Donation
{
    [Key]
    [Column("donation_id")]
    public long DonationId { get; set; }

    [Column("donor_name")]
    public string DonorName { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [Column("method")]
    public string Method { get; set; } = string.Empty;

    [Column("currency")]
    public string Currency { get; set; } = string.Empty;

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("donated_at")]
    public DateOnly DonatedAt { get; set; }

    [Column("message")]
    public string? Message { get; set; }

    [Column("proof_file")]
    public string ProofFile { get; set; } = string.Empty;

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
