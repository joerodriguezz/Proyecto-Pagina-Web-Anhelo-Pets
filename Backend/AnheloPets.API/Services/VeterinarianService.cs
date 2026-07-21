using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Repository;

namespace AnheloPets.API.Services;

public class VeterinarianService : IVeterinarianService
{
    private readonly VeterinarianRepository _repository;

    public VeterinarianService(VeterinarianRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<VeterinarianDto>> GetAll()
    {
        return await _repository.GetAll();
    }

    public async Task<VeterinarianDto?> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ApiException("El ID de veterinario no es válido.", 400);

        var result = await _repository.GetById(id);
        if (result == null)
            throw new ApiException($"No se encontró el veterinario con ID {id}.", 404);

        return result;
    }

    public async Task<VeterinarianDto> Create(CreateVeterinarianDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName))
            throw new ApiException("El nombre del veterinario es obligatorio.", 400);

        if (string.IsNullOrWhiteSpace(dto.LastName))
            throw new ApiException("El apellido del veterinario es obligatorio.", 400);

        if (string.IsNullOrWhiteSpace(dto.Specialty))
            throw new ApiException("La especialidad es obligatoria.", 400);

        var existentes = await _repository.GetAll();
        var nombreCompleto = $"{dto.FirstName.Trim()} {dto.LastName.Trim()}";

        if (existentes.Any(v => string.Equals(v.FullName, nombreCompleto, StringComparison.OrdinalIgnoreCase)))
            throw new ApiException($"Ya existe un veterinario llamado {nombreCompleto}.", 409);

        return await _repository.Create(dto);
    }

    public async Task<VeterinarianDto?> Update(string id, UpdateVeterinarianDto dto)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ApiException("El ID de veterinario no es válido.", 400);

        if (string.IsNullOrWhiteSpace(dto.Specialty))
            throw new ApiException("La especialidad es obligatoria.", 400);

        var result = await _repository.Update(id, dto);
        if (result == null)
            throw new ApiException($"No se encontró el veterinario con ID {id}.", 404);

        return result;
    }

    public async Task<bool> Deactivate(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ApiException("El ID de veterinario no es válido.", 400);

        var result = await _repository.Deactivate(id);
        if (!result)
            throw new ApiException($"No se encontró el veterinario con ID {id}.", 404);

        return true;
    }
}
