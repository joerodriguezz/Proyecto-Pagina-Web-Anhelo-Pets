using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public class AnimalService : IAnimalService
{
    private static List<AnimalDto> _animals =
    [
        new()
        {
            AnimalId = 1,
            AnimalName = "Bartolo",
            Species = "Perro",
            AnimalStatus = "Disponible"
        }
    ];

    public IEnumerable<AnimalDto> GetAll()
    {
        return _animals;
    }

    public AnimalDto? GetById(long id)
    {
        return _animals.FirstOrDefault(x => x.AnimalId == id);
    }

    public AnimalDto Create(AnimalDto animal)
    {
        animal.AnimalId = _animals.Max(x => x.AnimalId) + 1;
        _animals.Add(animal);

        return animal;
    }

    public AnimalDto? Update(long id, AnimalDto animal)
    {
        var existing = _animals.FirstOrDefault(x => x.AnimalId == id);

        if (existing == null)
            return null;

        existing.AnimalName = animal.AnimalName;
        existing.Species = animal.Species;
        existing.AnimalStatus = animal.AnimalStatus;

        return existing;
    }

    public bool Delete(long id)
    {
        var animal = _animals.FirstOrDefault(x => x.AnimalId == id);

        if (animal == null)
            return false;

        _animals.Remove(animal);

        return true;
    }
}