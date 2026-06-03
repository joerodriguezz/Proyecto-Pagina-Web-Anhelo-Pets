namespace AnheloPets.API.DTOs;

public class AnimalDto
{
    public long AnimalId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string AnimalStatus { get; set; } = string.Empty;
}
