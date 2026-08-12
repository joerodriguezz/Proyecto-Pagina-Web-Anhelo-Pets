using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Repository;

namespace AnheloPets.API.Services;

public class VolunteerService : IVolunteerService
{
    private readonly VolunteerRepository _repository;

    public VolunteerService(VolunteerRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<VolunteerDto>> GetAll()
    {
        return await _repository.GetAll();
    }

    public async Task<VolunteerDto> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ApiException("El ID de voluntario no es válido.", 400);

        var result = await _repository.GetById(id);
        if (result == null)
            throw new ApiException($"No se encontró la solicitud con ID {id}.", 404);

        return result;
    }

    public async Task<VolunteerDto?> GetByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ApiException("El correo no es válido.", 400);

        return await _repository.GetByEmail(email);
    }

    public async Task<VolunteerDto> Submit(SubmitVolunteerApplicationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ApiException("El correo es obligatorio.", 400);

        if (string.IsNullOrWhiteSpace(dto.NationalId))
            throw new ApiException("La cédula es obligatoria.", 400);

        if (string.IsNullOrWhiteSpace(dto.VolunteerType))
            throw new ApiException("El tipo de voluntariado es obligatorio.", 400);

        if (string.IsNullOrWhiteSpace(dto.PhonePrimary))
            throw new ApiException("El teléfono es obligatorio.", 400);

        return await _repository.Submit(dto);
    }

    public async Task<VolunteerDto> CreateApproved(CreateApprovedVolunteerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName))
            throw new ApiException("El nombre es obligatorio.", 400);

        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ApiException("El correo es obligatorio.", 400);

        if (string.IsNullOrWhiteSpace(dto.PhonePrimary))
            throw new ApiException("El teléfono es obligatorio.", 400);

        if (string.IsNullOrWhiteSpace(dto.NationalId))
            throw new ApiException("La cédula es obligatoria.", 400);

        if (string.IsNullOrWhiteSpace(dto.VolunteerType))
            throw new ApiException("El tipo de voluntariado es obligatorio.", 400);

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
            throw new ApiException("La contraseña debe tener al menos 8 caracteres.", 400);

        return await _repository.CreateApproved(dto);
    }

    public async Task<VolunteerDto> Update(string id, UpdateVolunteerDto dto)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ApiException("El ID de voluntario no es válido.", 400);

        var result = await _repository.Update(id, dto);
        if (result == null)
            throw new ApiException($"No se encontró la solicitud con ID {id}.", 404);

        return result;
    }

    public async Task<VolunteerDto> UpdateStatus(string id, UpdateVolunteerStatusDto dto)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ApiException("El ID de voluntario no es válido.", 400);

        if (string.IsNullOrWhiteSpace(dto.Action))
            throw new ApiException("La acción es obligatoria.", 400);

        var result = await _repository.UpdateStatus(id, dto);
        if (result == null)
            throw new ApiException($"No se encontró la solicitud con ID {id}.", 404);

        return result;
    }
}
