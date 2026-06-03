using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IRescateService
{
    IEnumerable<RescateDto> GetAll();
    RescateDto? GetById(long id);
    RescateDto Create(RescateDto rescate);
    RescateDto? Update(long id, RescateDto rescate);
    bool Delete(long id);
}
