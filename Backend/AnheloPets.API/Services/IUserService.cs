using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IUserService
{
    AuthUserDto Register(RegisterUserDto request);
    AuthUserDto? Login(LoginDto request);
    bool UpdatePassword(long userId, PasswordUpdateDto request);
}
