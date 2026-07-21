namespace AnheloPets.API.DTOs;

public class AnimalDto
{
    public string? AnimalId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public int? AgeYears { get; set; }
    public string? Sex { get; set; }
    public string AnimalStatus { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? PhotoDescription { get; set; }
}
