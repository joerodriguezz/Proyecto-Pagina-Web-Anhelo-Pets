namespace AnheloPets.API.DTOs;

public class AnimalDto
{
    public long AnimalId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public int? AgeYears { get; set; }
    public int? AgeMonths { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string AnimalStatus { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string PhotoDescription { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "api";
    public string ModifiedBy { get; set; } = "api";
}
