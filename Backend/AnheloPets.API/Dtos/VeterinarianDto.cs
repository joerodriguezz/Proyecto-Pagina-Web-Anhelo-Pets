using System.ComponentModel.DataAnnotations;

namespace AnheloPets.API.DTOs;

/// <summary>
/// Vista de lectura de un veterinario. El nombre y la cédula no viven en
/// veterinarians: se traen de user_profiles a través de volunteers.
/// </summary>
public class VeterinarianDto
{
    public string? VeterinarianId { get; set; }

    public string VolunteerId { get; set; } = string.Empty;

    public string Specialty { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string? NationalId { get; set; }

    /// <summary>Estado de validación del voluntario asociado.</summary>
    public string ValidationStatus { get; set; } = "Pendiente";

    public bool Active { get; set; } = true;
}

/// <summary>
/// Alta en cascada: crea user + user_profile + volunteer + veterinarian
/// en una sola transacción.
/// </summary>
public class CreateVeterinarianDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, ErrorMessage = "El apellido no puede superar 100 caracteres.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especialidad es obligatoria.")]
    [StringLength(100, ErrorMessage = "La especialidad no puede superar 100 caracteres.")]
    public string Specialty { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "La cédula no puede superar 50 caracteres.")]
    public string? NationalId { get; set; }

    /// <summary>
    /// Opcional. Se usa como username del usuario creado; si no viene se
    /// deriva del nombre. users.username tiene restricción UNIQUE.
    /// </summary>
    [StringLength(100, ErrorMessage = "El correo no puede superar 100 caracteres.")]
    public string? Email { get; set; }

    public string? Nationality { get; set; }

    public string CreatedBy { get; set; } = "api";
}

/// <summary>Actualización: solo lo que es propio de la entidad veterinarian.</summary>
public class UpdateVeterinarianDto
{
    [Required(ErrorMessage = "La especialidad es obligatoria.")]
    [StringLength(100, ErrorMessage = "La especialidad no puede superar 100 caracteres.")]
    public string Specialty { get; set; } = string.Empty;

    public string ModifiedBy { get; set; } = "api";
}
