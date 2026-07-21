using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IUserAdminService
{
    Task<List<UserAdminDto>> GetAll();
    Task<UserAdminDto?> GetById(string userId);
    Task<UserAdminDto> UpdateStatus(string userId, UpdateUserStatusDto status);
    Task<UserAdminDto> SetRoles(string userId, UpdateUserRolesDto roles);
}
