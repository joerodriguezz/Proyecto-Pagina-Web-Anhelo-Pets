using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

public class FosterHome
{
    [Key]
    [Column("foster_home_id")]
    public string? FosterHomeId { get; set; }

    [Column("volunteer_id")]
    public string? VolunteerId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("address")]
    public string Address { get; set; } = string.Empty;

    [Column("phone")]
    public string Phone { get; set; } = string.Empty;

    [Column("responsible")]
    public string Responsible { get; set; } = string.Empty;

    [Column("capacity")]
    public int Capacity { get; set; } = 1;

    [Column("active")]
    public bool Active { get; set; } = true;

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
