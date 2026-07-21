using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

public class RescueRecord
{
    [Key]
    [Column("rescue_id")]
    public long RescueId { get; set; }

    [Column("animal_id")]
    public string? AnimalId { get; set; }

    [Column("rescue_date")]
    public DateOnly RescueDate { get; set; }

    [Column("location")]
    public string Location { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = "Activo";

    [Column("foster_home_id")]
    public string? FosterHomeId { get; set; }

    [Column("volunteer_id")]
    public string? VolunteerId { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
}
