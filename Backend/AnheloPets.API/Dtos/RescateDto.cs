namespace AnheloPets.API.DTOs;

public class RescateDto
{
    public long RescateId { get; set; }
    public DateTime Fecha { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
