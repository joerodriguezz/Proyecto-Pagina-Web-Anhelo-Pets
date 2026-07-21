namespace AnheloPets.API.DTOs;

public class UserAdminDto
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhonePrimary { get; set; } = string.Empty;
    public string? PhoneSecondary { get; set; }
    public string? NationalId { get; set; }
    public string? Nationality { get; set; }
    public string? City { get; set; }
    public string? Town { get; set; }
    public string? AddressLine { get; set; }
    public bool Active { get; set; }
    public DateTime? CreatedAt { get; set; }

    public bool IsVolunteer { get; set; }
    public string? VolunteerType { get; set; }
    public string? VolunteerValidationStatus { get; set; }

    public List<RoleDto> Roles { get; set; } = new();
}

public class UpdateUserStatusDto
{
    public bool Active { get; set; }
    public string ModifiedBy { get; set; } = "admin";
}

public class UpdateUserRolesDto
{
    public long[] RoleIds { get; set; } = Array.Empty<long>();
    public string ModifiedBy { get; set; } = "admin";
}
