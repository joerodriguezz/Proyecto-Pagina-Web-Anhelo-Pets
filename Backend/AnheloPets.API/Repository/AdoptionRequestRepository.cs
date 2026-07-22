using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Repository;

public class AdoptionRequestRepository
{
    private static readonly HashSet<string> AllowedActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Proceso", "Aprobar", "Rechazar"
    };

    private readonly AnheloPetsDbContext _context;

    public AdoptionRequestRepository(AnheloPetsDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdoptionRequestDto>> GetAllAsync()
    {
        return await _context.AdoptionRequests
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => ToDto(r))
            .ToListAsync();
    }

    public async Task<AdoptionRequestDto?> GetByIdAsync(string id)
    {
        var request = await _context.AdoptionRequests.FindAsync(id);
        return request == null ? null : ToDto(request);
    }

    public async Task<AdoptionRequestDto> CreateAsync(string userId, SubmitAdoptionRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.AnimalId))
            throw new BadRequestException("La mascota es obligatoria.");

        var animalExists = await _context.Animals.AnyAsync(a => a.AnimalId == dto.AnimalId);
        if (!animalExists)
            throw new NotFoundException("La mascota indicada no existe.");

        var duplicate = await _context.AdoptionRequests
            .AnyAsync(r => r.UserId == userId && r.AnimalId == dto.AnimalId);
        if (duplicate)
            throw new ApiException("Ya enviaste una solicitud para esta mascota.", 409, "DUPLICATE_ADOPTION_REQUEST");

        var request = new AdoptionRequest
        {
            UserId = userId,
            AnimalId = dto.AnimalId,
            ApplicantName = dto.ApplicantName.Trim(),
            NationalId = dto.NationalId.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone.Trim(),
            Age = dto.Age,
            HasWhatsapp = dto.HasWhatsapp,
            LivesInCostaRica = dto.LivesInCostaRica,
            ForeignCountry = dto.LivesInCostaRica ? null : dto.ForeignCountry,
            Address = dto.Address.Trim(),
            PetNameSnapshot = dto.PetNameSnapshot,
            ReasonForPet = dto.ReasonForPet,
            AdoptionReasons = dto.AdoptionReasons,
            HouseholdMembers = dto.HouseholdMembers,
            OtherPets = dto.OtherPets,
            Profession = dto.Profession.Trim(),
            DailyRoutine = dto.DailyRoutine,
            HoursAlone = dto.HoursAlone,
            ValidationStatus = "Pendiente",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
        };

        try
        {
            _context.AdoptionRequests.Add(request);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Respaldo final ante una carrera entre el chequeo AnyAsync y el insert:
            // uq_adoption_requests_user_animal es la garantía real de "sin duplicados".
            throw new ApiException("Ya enviaste una solicitud para esta mascota.", 409, "DUPLICATE_ADOPTION_REQUEST");
        }

        return ToDto(request);
    }

    /// <summary>
    /// Proceso -> animal 'En proceso' | Aprobar -> animal 'Adoptada' | Rechazar -> animal 'Disponible'.
    /// Ambas escrituras (solicitud + animal) viajan en el mismo SaveChangesAsync para atomicidad real.
    /// </summary>
    public async Task<AdoptionRequestDto?> UpdateStatusAsync(string id, UpdateAdoptionRequestStatusDto dto)
    {
        if (!AllowedActions.Contains(dto.Action))
            throw new BadRequestException($"Acción inválida: {dto.Action}.");

        var request = await _context.AdoptionRequests.FindAsync(id);
        if (request == null) return null;

        var animal = await _context.Animals.FindAsync(request.AnimalId);
        var actor = string.IsNullOrWhiteSpace(dto.ModifiedBy) ? "admin" : dto.ModifiedBy;

        switch (dto.Action.ToLowerInvariant())
        {
            case "proceso":
                request.ValidationStatus = "En proceso";
                if (animal != null) animal.AnimalStatus = "En proceso";
                break;
            case "aprobar":
                request.ValidationStatus = "Aprobada";
                request.ValidatedAt = DateTime.UtcNow;
                if (animal != null) animal.AnimalStatus = "Adoptada";
                break;
            case "rechazar":
                request.ValidationStatus = "Rechazada";
                request.ValidatedAt = DateTime.UtcNow;
                if (animal != null) animal.AnimalStatus = "Disponible";
                break;
        }

        if (dto.ValidationNotes != null) request.ValidationNotes = dto.ValidationNotes;
        request.ValidatedByUserId = actor;
        request.ModifiedAt = DateTime.UtcNow;
        request.ModifiedBy = actor;

        await _context.SaveChangesAsync();

        return ToDto(request);
    }

    private static AdoptionRequestDto ToDto(AdoptionRequest r) => new()
    {
        AdoptionRequestId = r.AdoptionRequestId ?? string.Empty,
        UserId = r.UserId,
        AnimalId = r.AnimalId,
        ApplicantName = r.ApplicantName,
        NationalId = r.NationalId,
        Email = r.Email,
        Phone = r.Phone,
        Age = r.Age,
        HasWhatsapp = r.HasWhatsapp,
        LivesInCostaRica = r.LivesInCostaRica,
        ForeignCountry = r.ForeignCountry,
        Address = r.Address,
        PetNameSnapshot = r.PetNameSnapshot,
        ReasonForPet = r.ReasonForPet,
        AdoptionReasons = r.AdoptionReasons,
        HouseholdMembers = r.HouseholdMembers,
        OtherPets = r.OtherPets,
        Profession = r.Profession,
        DailyRoutine = r.DailyRoutine,
        HoursAlone = r.HoursAlone,
        ValidationStatus = r.ValidationStatus,
        ValidationNotes = r.ValidationNotes,
        ValidatedAt = r.ValidatedAt,
        CreatedAt = r.CreatedAt,
    };
}
