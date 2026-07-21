using System.ComponentModel.DataAnnotations;

namespace AnheloPets.API.DTOs;

public class FosterHomeDto
{
    public string? FosterHomeId { get; set; }

    public string? VolunteerId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(500, ErrorMessage = "La dirección no puede superar 500 caracteres.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(20, ErrorMessage = "El teléfono no puede superar 20 caracteres.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "El responsable es obligatorio.")]
    [StringLength(100, ErrorMessage = "El responsable no puede superar 100 caracteres.")]
    public string Responsible { get; set; } = string.Empty;

    [Range(1, 50, ErrorMessage = "La capacidad debe ser entre 1 y 50.")]
    public int Capacity { get; set; } = 1;

    public bool Active { get; set; } = true;

    public string CreatedBy { get; set; } = "api";
    public string ModifiedBy { get; set; } = "api";
}
