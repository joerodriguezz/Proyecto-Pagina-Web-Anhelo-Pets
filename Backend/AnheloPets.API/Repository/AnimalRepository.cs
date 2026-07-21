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
                Description = dto.Description,
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
        entity.DateOfBirth = dto.BirthDate;
        entity.Gender = dto.Sex?.FirstOrDefault() ?? char.MinValue;
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
            AnimalStatus = entity.AnimalStatus,
            HealthStatus = entity.HealthStatus,
            Description = entity.Description ?? string.Empty,
        };
    }

}