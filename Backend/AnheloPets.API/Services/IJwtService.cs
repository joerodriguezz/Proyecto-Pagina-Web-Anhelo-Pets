using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IJwtService
{
    string GenerateToken(AuthUserDto user);
}
