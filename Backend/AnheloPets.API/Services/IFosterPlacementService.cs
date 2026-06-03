using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IFosterPlacementService
{
    IEnumerable<FosterPlacementDto> GetAll();
    FosterPlacementDto? GetById(long id);
    FosterPlacementDto Create(FosterPlacementDto placement);
    FosterPlacementDto? Update(long id, FosterPlacementDto placement);
    bool Delete(long id);
}
