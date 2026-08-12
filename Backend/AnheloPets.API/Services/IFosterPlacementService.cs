using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IFosterPlacementService
{
    Task<List<FosterPlacementDto>> GetAll();
    Task<FosterPlacementDto?> GetById(long id);
    Task<FosterPlacementDto?> GetActiveByAnimalId(string animalId);
    Task<FosterPlacementDto> Create(FosterPlacementDto placement);
    Task<FosterPlacementDto?> Update(long id, FosterPlacementDto placement);
    Task<bool> Delete(long id);
}
