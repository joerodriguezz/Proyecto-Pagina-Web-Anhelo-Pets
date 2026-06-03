using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IAnimalService
{
    IEnumerable<AnimalDto> GetAll();
    AnimalDto? GetById(long id);
    AnimalDto Create(AnimalDto animal);
    AnimalDto? Update(long id, AnimalDto animal);
    bool Delete(long id);
}