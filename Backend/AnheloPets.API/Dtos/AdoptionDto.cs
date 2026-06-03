namespace AnheloPets.API.DTOs;

public class AdoptionDto
{
    public long AdoptionId { get; set; }
    public long AnimalId { get; set; }
    public string AdopterName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
    public string Notes { get; set; } = string.Empty;
}
