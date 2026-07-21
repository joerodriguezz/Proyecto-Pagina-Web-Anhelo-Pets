using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IRoleService
{
    Task<List<RoleDto>> GetAll();
    Task<RoleDto?> GetById(long id);
    Task<RoleDto> Create(CreateRoleDto role);
    Task<RoleDto?> Update(long id, UpdateRoleDto role);
    Task<bool> Delete(long id);
}
