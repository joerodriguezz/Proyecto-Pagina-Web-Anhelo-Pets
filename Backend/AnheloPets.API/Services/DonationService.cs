using AnheloPets.API.DTOs;
using AnheloPets.API.Repository;

namespace AnheloPets.API.Services;

public class DonationService : IDonationService
{
    private readonly DonationRepository _repository;

    public DonationService(DonationRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<DonationDto>> GetAllAsync() => _repository.GetAllAsync();

    public Task<DonationDto?> GetByIdAsync(long id) => _repository.GetByIdAsync(id);

    public Task<DonationDto> CreateAsync(SubmitDonationDto donation) => _repository.CreateAsync(donation);

    public Task<DonationDto> UpdateStatusAsync(long id, UpdateDonationStatusDto status) => _repository.UpdateStatusAsync(id, status);
}
