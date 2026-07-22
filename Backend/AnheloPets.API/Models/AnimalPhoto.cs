using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnheloPets.API.Models;

[Table("animal_photos")]
public class AnimalPhoto
{
    [Key]
    [Column("animal_photo_id")]
    public long AnimalPhotoId { get; set; }

    [Column("animal_id")]
    [ForeignKey("Animal")]
    public string AnimalId { get; set; } = string.Empty;

    public Animal? Animal { get; set; }

    [Column("photo_url")]
    public string PhotoUrl { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    [Column("display_order")]
    public int DisplayOrder { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("created_by")]
    public string? CreatedBy { get; set; }

    [Column("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }
}
