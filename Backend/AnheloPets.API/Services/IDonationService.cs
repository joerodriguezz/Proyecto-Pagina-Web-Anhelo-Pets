using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IDonationService
{
    IEnumerable<DonationDto> GetAll();
    DonationDto? GetById(long id);
    DonationDto Create(DonationDto donation);
    DonationDto? Update(long id, DonationDto donation);
    bool Delete(long id);
}
