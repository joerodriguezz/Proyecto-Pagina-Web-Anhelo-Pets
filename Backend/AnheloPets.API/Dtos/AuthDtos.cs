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

public class LoginDtoResponse
{
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class LoginDtoRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class PasswordUpdateDto
{
    public string Password { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = "api";
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
}
