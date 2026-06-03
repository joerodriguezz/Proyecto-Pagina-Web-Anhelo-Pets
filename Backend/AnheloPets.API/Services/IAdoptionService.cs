using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IAdoptionService
{
    IEnumerable<AdoptionDto> GetAll();
    AdoptionDto? GetById(long id);
    AdoptionDto Create(AdoptionDto adoption);
    AdoptionDto? Update(long id, AdoptionDto adoption);
    bool Delete(long id);
}
