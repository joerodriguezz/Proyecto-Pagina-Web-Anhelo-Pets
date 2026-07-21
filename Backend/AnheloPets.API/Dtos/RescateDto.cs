namespace AnheloPets.API.DTOs;

public class RescateDto
{
    public long RescateId { get; set; }

    public string? AnimalId { get; set; }

    public string AnimalName { get; set; } = string.Empty;

    public DateOnly? Fecha { get; set; }

    public string Ubicacion { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Status { get; set; } = "Activo";

    public string? FosterHomeId { get; set; }

    public string FosterHomeName { get; set; } = string.Empty;

    public string? VolunteerId { get; set; }

    public string? VolunteerName { get; set; }

    public string CreatedBy { get; set; } = "api";
    public string ModifiedBy { get; set; } = "api";
}
