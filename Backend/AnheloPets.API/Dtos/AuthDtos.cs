namespace AnheloPets.API.DTOs;

public class RegisterUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string SecondLastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhonePrimary { get; set; } = string.Empty;
    public string PhoneSecondary { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "api";
}

public class LoginDto
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class PasswordUpdateDto
{
    public string Password { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = "api";
}

public class AuthUserDto
{
    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsVolunteer { get; set; }
    public bool VolunteerActive { get; set; }
    public string VolunteerValidationStatus { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
}
