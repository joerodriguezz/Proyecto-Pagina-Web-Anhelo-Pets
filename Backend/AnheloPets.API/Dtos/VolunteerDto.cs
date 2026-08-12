using System.ComponentModel.DataAnnotations;

namespace AnheloPets.API.DTOs;

/// <summary>
/// Vista de lectura de una solicitud de voluntariado. El nombre y los datos de
/// contacto no viven en volunteers: se traen de user_profiles/user_contacts a
/// través de volunteers.user_id.
/// </summary>
public class VolunteerDto
{
    public string VolunteerId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? VolunteerType { get; set; }
    public string? Motivation { get; set; }

    /// <summary>JSON crudo con las respuestas del cuestionario específico del tipo elegido.</summary>
    public string? ApplicationDetails { get; set; }

    public string? Email { get; set; }
    public string? PhonePrimary { get; set; }
    public string? City { get; set; }
    public string? Town { get; set; }
    public string? District { get; set; }

    public bool Active { get; set; }
    public string ValidationStatus { get; set; } = "Pendiente";
    public string? ValidationNotes { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// Envío público del formulario. Requiere que ya exista una cuenta con este correo
/// (creada vía /api/auth/register): el voluntariado se resuelve contra un usuario
/// real, no crea uno nuevo.
/// </summary>
public class SubmitVolunteerApplicationDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [StringLength(50, ErrorMessage = "La cédula no puede superar 50 caracteres.")]
    public string NationalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de voluntariado es obligatorio.")]
    [StringLength(100, ErrorMessage = "El tipo no puede superar 100 caracteres.")]
    public string VolunteerType { get; set; } = string.Empty;

    public string? Motivation { get; set; }
    public string? ApplicationDetails { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(30, ErrorMessage = "El teléfono no puede superar 30 caracteres.")]
    public string PhonePrimary { get; set; } = string.Empty;

    public string? City { get; set; }
    public string? Town { get; set; }
    public string? District { get; set; }

    public string CreatedBy { get; set; } = "public";
}

/// <summary>Edición administrativa de una solicitud existente.</summary>
public class UpdateVolunteerDto
{
    public string? NationalId { get; set; }
    public string? VolunteerType { get; set; }
    public string? Motivation { get; set; }
    public string? ApplicationDetails { get; set; }
    public string? PhonePrimary { get; set; }
    public string? City { get; set; }
    public string? Town { get; set; }
    public string? District { get; set; }

    public string ModifiedBy { get; set; } = "admin";
}

/// <summary>
/// Alta administrativa desde el panel: crea la cuenta (users/user_profiles/user_contacts)
/// y la solicitud de voluntariado ya aprobada en una sola operación, a diferencia de
/// Submit() que exige que el usuario ya exista. Pensado para flujos de "alta rápida"
/// como Rescatista desde RescatesAdminView.vue.
/// </summary>
public class CreateApprovedVolunteerDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El apellido no puede superar 100 caracteres.")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(30, ErrorMessage = "El teléfono no puede superar 30 caracteres.")]
    public string PhonePrimary { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [StringLength(50, ErrorMessage = "La cédula no puede superar 50 caracteres.")]
    public string NationalId { get; set; } = string.Empty;

    public string? Nationality { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de voluntariado es obligatorio.")]
    [StringLength(100, ErrorMessage = "El tipo no puede superar 100 caracteres.")]
    public string VolunteerType { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = "admin";
}

/// <summary>Acción administrativa sobre el estado de una solicitud.</summary>
public class UpdateVolunteerStatusDto
{
    /// <summary>Aprobar | Rechazar | Inactivar | Reactivar.</summary>
    [Required(ErrorMessage = "La acción es obligatoria.")]
    public string Action { get; set; } = string.Empty;

    public string? ValidationNotes { get; set; }
    public string ModifiedBy { get; set; } = "admin";
}
