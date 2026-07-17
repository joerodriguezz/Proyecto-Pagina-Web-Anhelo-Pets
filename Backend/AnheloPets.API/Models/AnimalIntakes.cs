namespace AnheloPets.API.Models;

public class AnimalIntakes
{
    public long? RescueId { get; set; }
    
    public string AnimalId { get; set; } = string.Empty;
    public string intakeType { get; set; } = string.Empty;
    public string reportedBy { get; set; } = string.Empty;
    public string? intakeAddress { get; set; }
    public string? notes { get; set; }
    public DateTime? intakeAt { get; set; }
}