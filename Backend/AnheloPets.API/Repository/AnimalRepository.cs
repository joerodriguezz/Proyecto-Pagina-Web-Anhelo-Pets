using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Repository;

public class AnimalRepository
{
    private readonly AnheloPetsDbContext _context;

    public AnimalRepository(AnheloPetsDbContext context)
    {
        _context = context;
    }

    public async Task<GetResponse> Create (AnimalDto dto)
    {
        try
        {
            Animal entidad = new Animal
            {
                Species = dto.Species,
                Breed = dto.Breed,
                AnimalName = dto.AnimalName,
                AnimalStatus = dto.AnimalStatus,
                HealthStatus = dto.HealthStatus,
                Gender = dto.Sex?.FirstOrDefault() ?? char.MinValue,
                Size = dto.Size,
                Personality = dto.Personality,
                Description = dto.Description,
                DateOfBirth = ComputeBirthDate(dto),
            };

            _context.Add(entidad);
            await _context.SaveChangesAsync();
            return new GetResponse
            {
                message = "Animal created successfully",
                id = entidad.AnimalId ??  string.Empty
            };
        }
        catch (Exception e)
        {
            throw new ApiException(e.Message, e);
        }
        
    }

    public async Task<AnimalDto?> Update(string id, AnimalDto dto)
    {
        var entity = await _context.Animals.FindAsync(id);
        if (entity == null) return null;

        entity.Species = dto.Species;
        entity.Breed = dto.Breed;
        entity.AnimalName = dto.AnimalName;
        entity.AnimalStatus = string.IsNullOrWhiteSpace(dto.AnimalStatus) ? "Disponible" : dto.AnimalStatus;
        entity.HealthStatus = string.IsNullOrWhiteSpace(dto.HealthStatus) ? "Pendiente" : dto.HealthStatus;
        entity.DateOfBirth = ComputeBirthDate(dto);
        entity.Gender = dto.Sex?.FirstOrDefault() ?? char.MinValue;
        entity.Size = dto.Size;
        entity.Personality = dto.Personality;
        entity.Description = dto.Description;

        await _context.SaveChangesAsync();

        return new AnimalDto
        {
            AnimalId = entity.AnimalId,
            AnimalName = entity.AnimalName ?? string.Empty,
            Species = entity.Species,
            Breed = entity.Breed ?? string.Empty,
            BirthDate = entity.DateOfBirth,
            Sex = entity.Gender != char.MinValue ? entity.Gender.ToString() : null,
            Size = entity.Size,
            Personality = entity.Personality,
            AnimalStatus = entity.AnimalStatus,
            HealthStatus = entity.HealthStatus,
            Description = entity.Description ?? string.Empty,
        };
    }

    /// <summary>
    /// El formulario solo captura "X años Y meses" (no una fecha exacta), así que la
    /// fecha de nacimiento se aproxima restando esa edad a hoy. Si el DTO ya trae una
    /// BirthDate explícita se respeta esa en vez de recalcularla.
    /// </summary>
    private static DateOnly? ComputeBirthDate(AnimalDto dto)
    {
        if (dto.BirthDate.HasValue) return dto.BirthDate;

        var years = dto.AgeYears ?? 0;
        var months = dto.AgeMonths ?? 0;
        if (years == 0 && months == 0) return null;

        return DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-years).AddMonths(-months);
    }

}