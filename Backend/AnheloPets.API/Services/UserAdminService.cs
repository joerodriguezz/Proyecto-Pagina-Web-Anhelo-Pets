using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Repository;

namespace AnheloPets.API.Services;

public class UserAdminService : IUserAdminService
{
    private readonly UserAdminRepository _repository;

    public UserAdminService(UserAdminRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserAdminDto>> GetAll()
    {
        return await _repository.GetAll();
    }

    public async Task<UserAdminDto?> GetById(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ApiException("El ID de usuario no es válido.", 400);

        var result = await _repository.GetById(userId);
        if (result == null)
            throw new ApiException($"No se encontró el usuario con ID {userId}.", 404);

        return result;
    }

    public async Task<UserAdminDto> UpdateStatus(string userId, UpdateUserStatusDto dto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ApiException("El ID de usuario no es válido.", 400);

        var result = await _repository.UpdateStatus(userId, dto.Active, dto.ModifiedBy);
        if (result == null)
            throw new ApiException($"No se encontró el usuario con ID {userId}.", 404);

        return result;
    }

    public async Task<UserAdminDto> SetRoles(string userId, UpdateUserRolesDto dto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ApiException("El ID de usuario no es válido.", 400);

        var result = await _repository.SetRoles(userId, dto.RoleIds, dto.ModifiedBy);
        if (result == null)
            throw new ApiException($"No se encontró el usuario con ID {userId}.", 404);

        return result;
    }
}
