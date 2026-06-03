using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public class DonationService : IDonationService
{
    private static List<DonationDto> _donations = new()
    {
        new()
        {
            DonationId = 1,
            DonorName = "María Pérez",
            Amount = 50m,
            DonatedAt = DateTime.UtcNow.AddDays(-1),
            Message = "Para el cuidado de los animales"
        }
    };

    public IEnumerable<DonationDto> GetAll() => _donations;

    public DonationDto? GetById(long id) => _donations.FirstOrDefault(x => x.DonationId == id);

    public DonationDto Create(DonationDto donation)
    {
        donation.DonationId = _donations.Any() ? _donations.Max(x => x.DonationId) + 1 : 1;
        _donations.Add(donation);
        return donation;
    }

    public DonationDto? Update(long id, DonationDto donation)
    {
        var existing = _donations.FirstOrDefault(x => x.DonationId == id);
        if (existing == null)
            return null;

        existing.DonorName = donation.DonorName;
        existing.Amount = donation.Amount;
        existing.DonatedAt = donation.DonatedAt;
        existing.Message = donation.Message;

        return existing;
    }

    public bool Delete(long id)
    {
        var donation = _donations.FirstOrDefault(x => x.DonationId == id);
        if (donation == null)
            return false;

        _donations.Remove(donation);
        return true;
    }
}
