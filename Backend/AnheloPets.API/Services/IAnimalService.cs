using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IAnimalService
{
    IEnumerable<AnimalDto> GetAll(string? species = null, string? status = "Disponible", string? search = null);
    AnimalDto? GetById(long id);
    Task<GetResponse> Create(AnimalDto animal);
    AnimalDto? Update(long id, AnimalDto animal);
}
