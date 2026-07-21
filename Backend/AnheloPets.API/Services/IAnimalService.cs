using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IAnimalService
{
    IEnumerable<AnimalDto> GetAll(string? species = null, string? status = "Disponible", string? search = null, string? column = null, string? value = null);
    AnimalDto? GetById(string id);
    Task<GetResponse> Create(AnimalDto animal);
    Task<AnimalDto?> Update(string id, AnimalDto animal);
    Task<AnimalDto?> ChangeStatus(string id, string status);
}
