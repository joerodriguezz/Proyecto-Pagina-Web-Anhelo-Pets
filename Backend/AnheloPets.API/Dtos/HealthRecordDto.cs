using System.ComponentModel.DataAnnotations;

namespace AnheloPets.API.DTOs;

public class AnimalMedicalRecordDto
{
    public long AnimalMedicalRecordId { get; set; }

    [Required(ErrorMessage = "La mascota es obligatoria.")]
    public string AnimalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El veterinario es obligatorio.")]
    public string VeterinarianId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El diagnóstico es obligatorio.")]
    public string Diagnosis { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tratamiento es obligatorio.")]
    public string Treatment { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateOnly VisitDate { get; set; }

    /// <summary>Nombre del veterinario, solo lectura (vive en user_profiles).</summary>
    public string? VeterinarianName { get; set; }

    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
}
