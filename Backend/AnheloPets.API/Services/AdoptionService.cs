using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public class AdoptionService : IAdoptionService
{
    private static List<AdoptionDto> _adoptions = new()
    {
        new()
        {
            AdoptionId = 1,
            AnimalId = 1,
            AdopterName = "Luis Mora",
            RequestedAt = DateTime.UtcNow.AddDays(-3),
            Status = "Pending",
            Notes = "Revisar si puede recibir visitas antes de la adopción"
        }
    };

    public IEnumerable<AdoptionDto> GetAll() => _adoptions;

    public AdoptionDto? GetById(long id) => _adoptions.FirstOrDefault(x => x.AdoptionId == id);

    public AdoptionDto Create(AdoptionDto adoption)
    {
        adoption.AdoptionId = _adoptions.Any() ? _adoptions.Max(x => x.AdoptionId) + 1 : 1;
        _adoptions.Add(adoption);
        return adoption;
    }

    public AdoptionDto? Update(long id, AdoptionDto adoption)
    {
        var existing = _adoptions.FirstOrDefault(x => x.AdoptionId == id);
        if (existing == null)
            return null;

        existing.AnimalId = adoption.AnimalId;
        existing.AdopterName = adoption.AdopterName;
        existing.RequestedAt = adoption.RequestedAt;
        existing.Status = adoption.Status;
        existing.Notes = adoption.Notes;

        return existing;
    }

    public bool Delete(long id)
    {
        var adoption = _adoptions.FirstOrDefault(x => x.AdoptionId == id);
        if (adoption == null)
            return false;

        _adoptions.Remove(adoption);
        return true;
    }
}
