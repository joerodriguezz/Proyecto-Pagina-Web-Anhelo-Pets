using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Repository;

public class AnimalFosterPlacementRepository
{
    private readonly AnheloPetsDbContext _context;

    public AnimalFosterPlacementRepository(AnheloPetsDbContext context)
    {
        _context = context;
    }

    public async Task<List<FosterPlacementDto>> GetAll()
    {
        return await BaseQuery()
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();
    }

    public async Task<FosterPlacementDto?> GetById(long id)
    {
        return await BaseQuery().FirstOrDefaultAsync(p => p.AnimalFosterPlacementId == id);
    }

    /// <summary>La asignación vigente de un animal (end_date null). Null si no tiene casa cuna asignada.</summary>
    public async Task<FosterPlacementDto?> GetActiveByAnimalId(string animalId)
    {
        return await BaseQuery()
            .Where(p => p.AnimalId == animalId && p.EndDate == null)
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<FosterPlacementDto> Create(FosterPlacementDto dto)
    {
        var entity = new AnimalFosterPlacement
        {
            AnimalId = dto.AnimalId,
            FosterHomeId = dto.FosterHomeId,
            StartDate = dto.StartDate == default ? DateOnly.FromDateTime(DateTime.UtcNow) : dto.StartDate,
            EndDate = dto.EndDate,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "api" : dto.CreatedBy,
        };

        _context.AnimalFosterPlacements.Add(entity);
        await _context.SaveChangesAsync();

        return (await GetById(entity.AnimalFosterPlacementId))!;
    }

    public async Task<FosterPlacementDto?> Update(long id, FosterPlacementDto dto)
    {
        var entity = await _context.AnimalFosterPlacements.FindAsync(id);
        if (entity == null) return null;

        entity.FosterHomeId = dto.FosterHomeId;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.Notes = dto.Notes;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = string.IsNullOrWhiteSpace(dto.ModifiedBy) ? "api" : dto.ModifiedBy;

        await _context.SaveChangesAsync();

        return await GetById(id);
    }

    /// <summary>Baja lógica: cierra la asignación con end_date = hoy, no borra la fila.</summary>
    public async Task<bool> Delete(long id)
    {
        var entity = await _context.AnimalFosterPlacements.FindAsync(id);
        if (entity == null) return false;

        entity.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = "api";

        await _context.SaveChangesAsync();
        return true;
    }

    private IQueryable<FosterPlacementDto> BaseQuery()
    {
        return from p in _context.AnimalFosterPlacements
               join a in _context.Animals on p.AnimalId equals a.AnimalId
               join f in _context.FosterHomes on p.FosterHomeId equals f.FosterHomeId
               select new FosterPlacementDto
               {
                   AnimalFosterPlacementId = p.AnimalFosterPlacementId,
                   AnimalId = p.AnimalId,
                   AnimalName = a.AnimalName ?? string.Empty,
                   FosterHomeId = p.FosterHomeId,
                   FosterHomeName = f.Name,
                   StartDate = p.StartDate,
                   EndDate = p.EndDate,
                   Notes = p.Notes,
               };
    }
}
