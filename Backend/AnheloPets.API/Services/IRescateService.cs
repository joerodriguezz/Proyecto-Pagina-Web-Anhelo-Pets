using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IRescateService
{
    Task<List<RescateDto>> GetAll();
    Task<RescateDto?> GetById(long id);
    Task<RescateDto> Create(RescateDto rescate);
    Task<RescateDto?> Update(long id, RescateDto rescate);
    Task<bool> Close(long id);
}
