using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IVolunteerService
{
    IEnumerable<VolunteerDto> GetAll();
    VolunteerDto? GetById(long id);
    VolunteerDto Create(VolunteerDto volunteer);
    VolunteerDto? Update(long id, VolunteerDto volunteer);
    bool Delete(long id);
}
