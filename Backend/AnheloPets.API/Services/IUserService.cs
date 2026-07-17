using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IUserService
{
    Task<AuthUserDto> Register(RegisterUserDto request);
    Task<LoginDtoResponse> Login(LoginDtoRequest request);
    bool UpdatePassword(long userId, PasswordUpdateDto request);
}
