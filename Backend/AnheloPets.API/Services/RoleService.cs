using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Repository;

namespace AnheloPets.API.Services;

public class RoleService : IRoleService
{
    private readonly RoleRepository _repository;

    public RoleService(RoleRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RoleDto>> GetAll()
    {
        return await _repository.GetAll();
    }

    public async Task<RoleDto?> GetById(long id)
    {
        var result = await _repository.GetById(id);
        if (result == null)
            throw new ApiException($"No se encontró el rol con ID {id}.", 404);

        return result;
    }

    public async Task<RoleDto> Create(CreateRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RoleName))
            throw new ApiException("El nombre del rol es obligatorio.", 400);

        if (string.IsNullOrWhiteSpace(dto.RoleAccess))
            throw new ApiException("El acceso del rol es obligatorio.", 400);

        return await _repository.Create(dto);
    }

    public async Task<RoleDto?> Update(long id, UpdateRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RoleName))
            throw new ApiException("El nombre del rol es obligatorio.", 400);

        if (string.IsNullOrWhiteSpace(dto.RoleAccess))
            throw new ApiException("El acceso del rol es obligatorio.", 400);

        var result = await _repository.Update(id, dto);
        if (result == null)
            throw new ApiException($"No se encontró el rol con ID {id}.", 404);

        return result;
    }

    public async Task<bool> Delete(long id)
    {
        var result = await _repository.Delete(id);
        if (!result)
            throw new ApiException($"No se encontró el rol con ID {id}.", 404);

        return true;
    }
}
