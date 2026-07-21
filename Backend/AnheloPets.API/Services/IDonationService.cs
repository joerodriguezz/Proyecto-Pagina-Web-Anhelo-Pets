using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IDonationService
{
    Task<IEnumerable<DonationDto>> GetAllAsync();
    Task<DonationDto?> GetByIdAsync(long id);
    Task<DonationDto> CreateAsync(SubmitDonationDto donation);
    Task<DonationDto> UpdateStatusAsync(long id, UpdateDonationStatusDto status);
}
