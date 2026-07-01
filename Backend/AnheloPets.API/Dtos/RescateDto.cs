using System.ComponentModel.DataAnnotations;

namespace AnheloPets.API.DTOs;

public class RescateDto
{
    public long RescateId { get; set; }

    public long? AnimalId { get; set; }

    public string AnimalName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha del rescate es obligatoria.")]
    public DateOnly Fecha { get; set; }

    [Required(ErrorMessage = "La ubicación es obligatoria.")]
    [StringLength(255, ErrorMessage = "La ubicación no puede superar 255 caracteres.")]
    public string Ubicacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(1000, ErrorMessage = "La descripción no puede superar 1000 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression("^(Activo|En proceso|Cerrado)$",
        ErrorMessage = "El estado debe ser: Activo, En proceso o Cerrado.")]
    public string Status { get; set; } = "Activo";

    public long? FosterHomeId { get; set; }

    public string FosterHomeName { get; set; } = string.Empty;

    public string? VolunteerName { get; set; }

    public string CreatedBy { get; set; } = "api";
    public string ModifiedBy { get; set; } = "api";
}