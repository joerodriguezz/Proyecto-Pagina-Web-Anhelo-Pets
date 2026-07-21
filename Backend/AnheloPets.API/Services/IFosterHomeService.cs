using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IFosterHomeService
{
    Task<List<FosterHomeDto>> GetAll();
    Task<FosterHomeDto?> GetById(string id);
    Task<FosterHomeDto> Create(FosterHomeDto fosterHome);
    Task<FosterHomeDto?> Update(string id, FosterHomeDto fosterHome);
    Task<bool> Deactivate(string id);
}
