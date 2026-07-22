namespace AnheloPets.API.DTOs;

public class AdoptionRequestDto
{
    public string AdoptionRequestId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string AnimalId { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool HasWhatsapp { get; set; }
    public bool LivesInCostaRica { get; set; }
    public string? ForeignCountry { get; set; }
    public string Address { get; set; } = string.Empty;
    public string PetNameSnapshot { get; set; } = string.Empty;
    public string? ReasonForPet { get; set; }
    public string AdoptionReasons { get; set; } = string.Empty;
    public string HouseholdMembers { get; set; } = string.Empty;
    public string? OtherPets { get; set; }
    public string Profession { get; set; } = string.Empty;
    public string DailyRoutine { get; set; } = string.Empty;
    public string HoursAlone { get; set; } = string.Empty;
    public string ValidationStatus { get; set; } = "Pendiente";
    public string? ValidationNotes { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

/// <summary>Envío público del formulario de adopción. UserId no viaja acá: se resuelve del JWT en el controller.</summary>
public class SubmitAdoptionRequestDto
{
    public string AnimalId { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool HasWhatsapp { get; set; }
    public bool LivesInCostaRica { get; set; } = true;
    public string? ForeignCountry { get; set; }
    public string Address { get; set; } = string.Empty;
    public string PetNameSnapshot { get; set; } = string.Empty;
    public string? ReasonForPet { get; set; }
    public string AdoptionReasons { get; set; } = string.Empty;
    public string HouseholdMembers { get; set; } = string.Empty;
    public string? OtherPets { get; set; }
    public string Profession { get; set; } = string.Empty;
    public string DailyRoutine { get; set; } = string.Empty;
    public string HoursAlone { get; set; } = string.Empty;
}

/// <summary>Acción administrativa: Proceso | Aprobar | Rechazar.</summary>
public class UpdateAdoptionRequestStatusDto
{
    public string Action { get; set; } = string.Empty;
    public string? ValidationNotes { get; set; }
    public string ModifiedBy { get; set; } = "admin";
}
