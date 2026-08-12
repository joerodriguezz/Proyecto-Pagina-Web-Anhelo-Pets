using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

[Table("animal_foster_placements")]
public class AnimalFosterPlacement
{
    [Key]
    [Column("animal_foster_placement_id")]
    public long AnimalFosterPlacementId { get; set; }

    [Column("animal_id")]
    public string AnimalId { get; set; } = string.Empty;

    [Column("foster_home_id")]
    public string FosterHomeId { get; set; } = string.Empty;

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
}
