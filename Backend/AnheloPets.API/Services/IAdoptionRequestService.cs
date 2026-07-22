using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IAdoptionRequestService
{
    Task<List<AdoptionRequestDto>> GetAllAsync();
    Task<AdoptionRequestDto?> GetByIdAsync(string id);
    Task<AdoptionRequestDto> CreateAsync(string userId, SubmitAdoptionRequestDto dto);
    Task<AdoptionRequestDto?> UpdateStatusAsync(string id, UpdateAdoptionRequestStatusDto dto);
}
