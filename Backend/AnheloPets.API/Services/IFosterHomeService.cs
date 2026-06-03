using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IFosterHomeService
{
    IEnumerable<FosterHomeDto> GetAll();
    FosterHomeDto? GetById(long id);
    FosterHomeDto Create(FosterHomeDto fosterHome);
    FosterHomeDto? Update(long id, FosterHomeDto fosterHome);
    bool Delete(long id);
}
