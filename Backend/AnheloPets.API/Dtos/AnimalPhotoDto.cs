namespace AnheloPets.API.DTOs;

public class AnimalPhotoDto
{
    public long AnimalPhotoId { get; set; }
    public string AnimalId { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}
