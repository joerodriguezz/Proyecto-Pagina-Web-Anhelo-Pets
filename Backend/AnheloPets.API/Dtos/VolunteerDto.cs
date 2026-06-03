namespace AnheloPets.API.DTOs;

public class VolunteerDto
{
    public long VolunteerId { get; set; }
    public long UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public bool? Active { get; set; }
    public string NationalId { get; set; } = string.Empty;
    public string VolunteerType { get; set; } = string.Empty;
    public string Motivation { get; set; } = string.Empty;
    public string ValidationStatus { get; set; } = string.Empty;
    public string ValidationNotes { get; set; } = string.Empty;
    public DateTime? ValidatedAt { get; set; }
    public long? ValidatedByUserId { get; set; }
    public string CreatedBy { get; set; } = "api";
    public string ModifiedBy { get; set; } = "api";
}
