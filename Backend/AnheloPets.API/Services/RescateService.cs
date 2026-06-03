using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public class RescateService : IRescateService
{
    private static List<RescateDto> _rescates = new()
    {
        new()
        {
            RescateId = 1,
            Fecha = DateTime.UtcNow.AddDays(-10),
            Ubicacion = "San José",
            Descripcion = "Rescatado en calle principal con heridas leves"
        }
    };

    public IEnumerable<RescateDto> GetAll() => _rescates;

    public RescateDto? GetById(long id) => _rescates.FirstOrDefault(x => x.RescateId == id);

    public RescateDto Create(RescateDto rescate)
    {
        rescate.RescateId = _rescates.Any() ? _rescates.Max(x => x.RescateId) + 1 : 1;
        _rescates.Add(rescate);
        return rescate;
    }

    public RescateDto? Update(long id, RescateDto rescate)
    {
        var existing = _rescates.FirstOrDefault(x => x.RescateId == id);
        if (existing == null)
            return null;

        existing.Fecha = rescate.Fecha;
        existing.Ubicacion = rescate.Ubicacion;
        existing.Descripcion = rescate.Descripcion;

        return existing;
    }

    public bool Delete(long id)
    {
        var rescate = _rescates.FirstOrDefault(x => x.RescateId == id);
        if (rescate == null)
            return false;

        _rescates.Remove(rescate);
        return true;
    }
}
