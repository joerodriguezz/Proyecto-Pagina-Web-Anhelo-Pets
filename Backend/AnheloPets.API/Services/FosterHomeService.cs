using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Repository;

namespace AnheloPets.API.Services;

public class FosterHomeService : IFosterHomeService
{
    private readonly FosterHomeRepository _repository;

    public FosterHomeService(FosterHomeRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<FosterHomeDto>> GetAll()
    {
        return await _repository.GetAll();
    }

    public async Task<FosterHomeDto?> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ApiException("El ID de casa cuna no es válido.");

        var result = await _repository.GetById(id);
        if (result == null)
            throw new ApiException($"No se encontró la casa cuna con ID {id}.", 404);

        return result;
    }

    public async Task<FosterHomeDto> Create(FosterHomeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ApiException("El nombre de la casa cuna es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Address))
            throw new ApiException("La dirección de la casa cuna es obligatoria.");

        if (string.IsNullOrWhiteSpace(dto.Phone))
            throw new ApiException("El teléfono de la casa cuna es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Responsible))
            throw new ApiException("El responsable de la casa cuna es obligatorio.");

        if (dto.Capacity < 1)
            throw new ApiException("La capacidad debe ser al menos 1.");

        return await _repository.Create(dto);
    }

    public async Task<FosterHomeDto?> Update(string id, FosterHomeDto dto)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ApiException("El ID de casa cuna no es válido.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ApiException("El nombre de la casa cuna es obligatorio.");

        var result = await _repository.Update(id, dto);
        if (result == null)
            throw new ApiException($"No se encontró la casa cuna con ID {id}.", 404);

        return result;
    }

    public async Task<bool> Deactivate(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ApiException("El ID de casa cuna no es válido.");

        var result = await _repository.Deactivate(id);
        if (!result)
            throw new ApiException($"No se encontró la casa cuna con ID {id}.", 404);

        return true;
    }
}
