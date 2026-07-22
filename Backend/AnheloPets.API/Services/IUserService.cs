using AnheloPets.API.DTOs;
using Microsoft.AspNetCore.Http;

namespace AnheloPets.API.Services;

public interface IUserService
{
    Task<AuthResponseDto> Register(RegisterUserDto request);
    Task<AuthResponseDto> Login(LoginDtoRequest request);
    Task<AuthResponseDto?> GetCurrentUser(string userId);
    Task ResetPasswordByEmail(ResetPasswordDto request);
    Task<AuthResponseDto?> UploadProfilePhoto(string userId, IFormFile file);
    Task<AuthResponseDto?> DeleteProfilePhoto(string userId);
}
