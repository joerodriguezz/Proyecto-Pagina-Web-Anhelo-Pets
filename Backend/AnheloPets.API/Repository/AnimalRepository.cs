using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Models;

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
                Gender = dto.Sex,
                Description = dto.Description,
            };

            _context.Add(entidad);
            await _context.SaveChangesAsync();
            return new GetResponse
            {
                message = "Animal created successfully"
            };
        }
        catch (Exception e)
        {
            throw new ApiException(e.Message, e);
        }
        
    }

}