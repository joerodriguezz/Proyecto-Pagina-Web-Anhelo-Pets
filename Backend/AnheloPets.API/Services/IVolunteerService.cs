using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IVolunteerService
{
    Task<List<VolunteerDto>> GetAll();
    Task<VolunteerDto> GetById(string id);
    Task<VolunteerDto?> GetByEmail(string email);
    Task<VolunteerDto> Submit(SubmitVolunteerApplicationDto application);
    Task<VolunteerDto> Update(string id, UpdateVolunteerDto volunteer);
    Task<VolunteerDto> UpdateStatus(string id, UpdateVolunteerStatusDto status);
}
