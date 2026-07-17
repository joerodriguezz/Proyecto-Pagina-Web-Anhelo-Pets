namespace AnheloPets.API.DTOs;

public class AnimalDto
{
    public string? AnimalId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public int? AgeYears { get; set; }
    public int? AgeMonths { get; set; }
    public char Sex { get; set; } = char.MinValue;
    public string AnimalStatus { get; set; } = string.Empty;
    public string HealthStatus { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string? PhotoDescription { get; set; } = string.Empty;
    public string? rescuetype { get; set; } = "Desconocido";
    public string? rescuename { get; set; } = "USR-999";
    public string? CreatedBy { get; set; } = "api";
    public string? ModifiedBy { get; set; } = "api";
}
