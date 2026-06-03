using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public class VolunteerService : IVolunteerService
{
    private static List<VolunteerDto> _volunteers = new()
    {
        new()
        {
            VolunteerId = 1,
            FirstName = "Ana",
            LastName = "González",
            Email = "ana.gonzalez@example.com",
            Phone = "+506 8888 7777"
        }
    };

    public IEnumerable<VolunteerDto> GetAll() => _volunteers;

    public VolunteerDto? GetById(long id) => _volunteers.FirstOrDefault(x => x.VolunteerId == id);

    public VolunteerDto Create(VolunteerDto volunteer)
    {
        volunteer.VolunteerId = _volunteers.Any() ? _volunteers.Max(x => x.VolunteerId) + 1 : 1;
        _volunteers.Add(volunteer);
        return volunteer;
    }

    public VolunteerDto? Update(long id, VolunteerDto volunteer)
    {
        var existing = _volunteers.FirstOrDefault(x => x.VolunteerId == id);
        if (existing == null)
            return null;

        existing.FirstName = volunteer.FirstName;
        existing.LastName = volunteer.LastName;
        existing.Email = volunteer.Email;
        existing.Phone = volunteer.Phone;

        return existing;
    }

    public bool Delete(long id)
    {
        var volunteer = _volunteers.FirstOrDefault(x => x.VolunteerId == id);
        if (volunteer == null)
            return false;

        _volunteers.Remove(volunteer);
        return true;
    }
}
