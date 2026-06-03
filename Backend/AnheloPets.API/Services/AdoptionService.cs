using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public class AdoptionService : IAdoptionService
{
    private static readonly List<AdoptionDto> Adoptions = new();

    public IEnumerable<AdoptionDto> GetAll() => Adoptions;

    public AdoptionDto? GetById(long id) => Adoptions.FirstOrDefault(x => x.AdoptionId == id);

    public AdoptionDto Create(AdoptionDto adoption)
    {
        adoption.AdoptionId = Adoptions.Any() ? Adoptions.Max(x => x.AdoptionId) + 1 : 1;
        Adoptions.Add(adoption);
        return adoption;
    }

    public AdoptionDto? Update(long id, AdoptionDto adoption)
    {
        var existing = Adoptions.FirstOrDefault(x => x.AdoptionId == id);
        if (existing == null)
        {
            return null;
        }

        existing.AnimalId = adoption.AnimalId;
        existing.AdopterName = adoption.AdopterName;
        existing.RequestedAt = adoption.RequestedAt;
        existing.Status = adoption.Status;
        existing.Notes = adoption.Notes;

        return existing;
    }

    public bool Delete(long id)
    {
        var adoption = Adoptions.FirstOrDefault(x => x.AdoptionId == id);
        if (adoption == null)
        {
            return false;
        }

        Adoptions.Remove(adoption);
        return true;
    }
}
