namespace AnheloPets.API.DTOs;

public class RescateDto
{
    public long RescateId { get; set; }
    public long? AnimalId { get; set; }
    public string AnimalName { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Status { get; set; } = "Activo";
    public long? FosterHomeId { get; set; }
    public string FosterHomeName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "api";
    public string ModifiedBy { get; set; } = "api";
}
