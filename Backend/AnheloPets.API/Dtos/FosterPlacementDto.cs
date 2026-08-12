using System.ComponentModel.DataAnnotations;

namespace AnheloPets.API.DTOs;

public class FosterPlacementDto
{
    public long AnimalFosterPlacementId { get; set; }

    [Required(ErrorMessage = "El animal es obligatorio.")]
    public string AnimalId { get; set; } = string.Empty;

    public string AnimalName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La casa cuna es obligatoria.")]
    public string FosterHomeId { get; set; } = string.Empty;

    public string FosterHomeName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [StringLength(1000, ErrorMessage = "Las notas no pueden superar 1000 caracteres.")]
    public string? Notes { get; set; }

    public string CreatedBy { get; set; } = "api";
    public string ModifiedBy { get; set; } = "api";
}
