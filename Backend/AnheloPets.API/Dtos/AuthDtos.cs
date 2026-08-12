namespace AnheloPets.API.DTOs;

public class RegisterUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhonePrimary { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string?[] Roles { get; set; } = [];
    public bool IsVolunteer { get; set; }
    public bool VolunteerActive { get; set; }
    public string? VolunteerValidationStatus { get; set; }
    public string? PhotoUrl { get; set; }
    public string? NationalId { get; set; }
    public string? PhonePrimary { get; set; }
}

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class LoginDtoRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthUserDto
{
    public string? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool? IsVolunteer { get; set; }
    public bool? VolunteerActive { get; set; }
    public string? VolunteerValidationStatus { get; set; } = string.Empty;
    public string?[] Roles { get; set; } = new string?[] { };
    public string? PhotoUrl { get; set; }
    public string? NationalId { get; set; }
    public string? PhonePrimary { get; set; }
}
