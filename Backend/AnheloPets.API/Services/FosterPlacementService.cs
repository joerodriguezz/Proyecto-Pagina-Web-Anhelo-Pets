using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Repository;

namespace AnheloPets.API.Services;

public class FosterPlacementService : IFosterPlacementService
{
    private readonly AnimalFosterPlacementRepository _repository;

    public FosterPlacementService(AnimalFosterPlacementRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<FosterPlacementDto>> GetAll() => await _repository.GetAll();

    public async Task<FosterPlacementDto?> GetById(long id) => await _repository.GetById(id);

    public async Task<FosterPlacementDto?> GetActiveByAnimalId(string animalId)
    {
        if (string.IsNullOrWhiteSpace(animalId))
            throw new ApiException("El ID del animal no es válido.", 400);

        return await _repository.GetActiveByAnimalId(animalId);
    }

    public async Task<FosterPlacementDto> Create(FosterPlacementDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.AnimalId))
            throw new ApiException("El animal es obligatorio.", 400);

        if (string.IsNullOrWhiteSpace(dto.FosterHomeId))
            throw new ApiException("La casa cuna es obligatoria.", 400);

        return await _repository.Create(dto);
    }

    public async Task<FosterPlacementDto?> Update(long id, FosterPlacementDto dto) => await _repository.Update(id, dto);

    public async Task<bool> Delete(long id) => await _repository.Delete(id);
}
