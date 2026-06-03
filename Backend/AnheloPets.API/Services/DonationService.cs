using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public class DonationService : IDonationService
{
    private static readonly List<DonationDto> Donations = new();

    public IEnumerable<DonationDto> GetAll() => Donations;

    public DonationDto? GetById(long id) => Donations.FirstOrDefault(x => x.DonationId == id);

    public DonationDto Create(DonationDto donation)
    {
        donation.DonationId = Donations.Any() ? Donations.Max(x => x.DonationId) + 1 : 1;
        Donations.Add(donation);
        return donation;
    }

    public DonationDto? Update(long id, DonationDto donation)
    {
        var existing = Donations.FirstOrDefault(x => x.DonationId == id);
        if (existing == null)
        {
            return null;
        }

        existing.DonorName = donation.DonorName;
        existing.Amount = donation.Amount;
        existing.DonatedAt = donation.DonatedAt;
        existing.Message = donation.Message;

        return existing;
    }

    public bool Delete(long id)
    {
        var donation = Donations.FirstOrDefault(x => x.DonationId == id);
        if (donation == null)
        {
            return false;
        }

        Donations.Remove(donation);
        return true;
    }
}
