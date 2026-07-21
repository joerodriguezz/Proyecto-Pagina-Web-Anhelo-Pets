using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IVeterinarianService
{
    Task<List<VeterinarianDto>> GetAll();
    Task<VeterinarianDto?> GetById(string id);
    Task<VeterinarianDto> Create(CreateVeterinarianDto veterinarian);
    Task<VeterinarianDto?> Update(string id, UpdateVeterinarianDto veterinarian);
    Task<bool> Deactivate(string id);
}
