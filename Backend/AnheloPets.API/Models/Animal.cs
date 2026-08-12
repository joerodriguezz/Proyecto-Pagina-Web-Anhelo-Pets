using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

public class Animal
{
    [Key]
    [Column("animal_id")]
    public string? AnimalId { get; set; }

    [Column("species")]
    public string Species { get; set; } = string.Empty;

    [Column("breed")]
    public string? Breed { get; set; }

    [Column("animal_name")]
    public string? AnimalName { get; set; }

    [Column("animal_status")]
    public string AnimalStatus { get; set; } = string.Empty;

    [Column("health_status")]
    public string HealthStatus { get; set; } = string.Empty;

    [Column("birth_date")]
    public DateOnly? DateOfBirth { get; set; }

    [Column("sex")]
    public char Gender { get; set; }

    [Column("size")]
    public string? Size { get; set; }

    [Column("description")]
    public string? Description { get; set; }

}
