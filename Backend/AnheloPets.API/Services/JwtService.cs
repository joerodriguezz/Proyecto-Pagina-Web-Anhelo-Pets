using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AnheloPets.API.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace AnheloPets.API.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(AuthUserDto user)
    {
        var signingKey = _configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey is not configured. Set Jwt__SigningKey in the deployment environment.");
        }

        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expiryMinutes = _configuration.GetValue("Jwt:ExpiryMinutes", 60);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.UserId ?? string.Empty),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new("is_volunteer", (user.IsVolunteer ?? false).ToString()),
            new("volunteer_active", (user.VolunteerActive ?? false).ToString()),
            new("volunteer_validation_status", user.VolunteerValidationStatus ?? string.Empty),
        };

        claims.AddRange((user.Roles ?? [])
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => new Claim(ClaimTypes.Role, role!)));

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
