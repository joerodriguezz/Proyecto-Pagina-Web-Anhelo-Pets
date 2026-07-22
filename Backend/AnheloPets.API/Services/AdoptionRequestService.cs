using AnheloPets.API.DTOs;
using AnheloPets.API.Repository;

namespace AnheloPets.API.Services;

public class AdoptionRequestService : IAdoptionRequestService
{
    private readonly AdoptionRequestRepository _repository;

    public AdoptionRequestService(AdoptionRequestRepository repository)
    {
        _repository = repository;
    }

    public Task<List<AdoptionRequestDto>> GetAllAsync() => _repository.GetAllAsync();

    public Task<AdoptionRequestDto?> GetByIdAsync(string id) => _repository.GetByIdAsync(id);

    public Task<AdoptionRequestDto> CreateAsync(string userId, SubmitAdoptionRequestDto dto) =>
        _repository.CreateAsync(userId, dto);

    public Task<AdoptionRequestDto?> UpdateStatusAsync(string id, UpdateAdoptionRequestStatusDto dto) =>
        _repository.UpdateStatusAsync(id, dto);
}
