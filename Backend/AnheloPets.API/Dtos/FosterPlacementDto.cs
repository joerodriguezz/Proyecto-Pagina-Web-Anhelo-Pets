namespace AnheloPets.API.DTOs;

public class FosterPlacementDto
{
    public long AnimalFosterPlacementId { get; set; }
    public long AnimalId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public long FosterHomeId { get; set; }
    public string FosterHomeName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "api";
    public string ModifiedBy { get; set; } = "api";
}
