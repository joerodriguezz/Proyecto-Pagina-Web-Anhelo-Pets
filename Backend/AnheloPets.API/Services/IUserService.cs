using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IUserService
{
    Task<AuthResponseDto> Register(RegisterUserDto request);
    Task<AuthResponseDto> Login(LoginDtoRequest request);
    Task<AuthResponseDto?> GetCurrentUser(string userId);
    Task ResetPasswordByEmail(ResetPasswordDto request);
}
