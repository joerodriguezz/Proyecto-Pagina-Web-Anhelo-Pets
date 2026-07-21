using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Repository;

public class RescueRepository
{
    private readonly AnheloPetsDbContext _context;

    public RescueRepository(AnheloPetsDbContext context)
    {
        _context = context;
    }

    public async Task<List<RescateDto>> GetAll()
    {
        return await _context.RescueRecords
            .OrderByDescending(r => r.RescueDate)
            .Select(r => new RescateDto
            {
                RescateId = r.RescueId,
                AnimalId = r.AnimalId,
                AnimalName = r.AnimalId != null
                    ? _context.Animals.Where(a => a.AnimalId == r.AnimalId).Select(a => a.AnimalName).FirstOrDefault() ?? ""
                    : "",
                Fecha = r.RescueDate,
                Ubicacion = r.Location,
                Descripcion = r.Description,
                Status = r.Status,
                FosterHomeId = r.FosterHomeId,
                FosterHomeName = r.FosterHomeId != null
                    ? _context.FosterHomes.Where(f => f.FosterHomeId == r.FosterHomeId).Select(f => f.Name).FirstOrDefault() ?? ""
                    : "",
                VolunteerId = r.VolunteerId,
            })
            .ToListAsync();
    }

    public async Task<RescateDto?> GetById(long id)
    {
        return await _context.RescueRecords
            .Where(r => r.RescueId == id)
            .Select(r => new RescateDto
            {
                RescateId = r.RescueId,
                AnimalId = r.AnimalId,
                AnimalName = r.AnimalId != null
                    ? _context.Animals.Where(a => a.AnimalId == r.AnimalId).Select(a => a.AnimalName).FirstOrDefault() ?? ""
                    : "",
                Fecha = r.RescueDate,
                Ubicacion = r.Location,
                Descripcion = r.Description,
                Status = r.Status,
                FosterHomeId = r.FosterHomeId,
                FosterHomeName = r.FosterHomeId != null
                    ? _context.FosterHomes.Where(f => f.FosterHomeId == r.FosterHomeId).Select(f => f.Name).FirstOrDefault() ?? ""
                    : "",
                VolunteerId = r.VolunteerId,
            })
            .FirstOrDefaultAsync();
    }

    public async Task<RescateDto> Create(RescateDto dto)
    {
        var entity = new RescueRecord
        {
            AnimalId = dto.AnimalId,
            RescueDate = dto.Fecha ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Location = dto.Ubicacion,
            Description = dto.Descripcion,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Activo" : dto.Status,
            FosterHomeId = dto.FosterHomeId,
            VolunteerId = dto.VolunteerId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "api" : dto.CreatedBy,
        };

        _context.RescueRecords.Add(entity);
        await _context.SaveChangesAsync();

        return (await GetById(entity.RescueId))!;
    }

    public async Task<RescateDto?> Update(long id, RescateDto dto)
    {
        var entity = await _context.RescueRecords.FindAsync(id);
        if (entity == null) return null;

        entity.AnimalId = dto.AnimalId;
        entity.RescueDate = dto.Fecha ?? entity.RescueDate;
        entity.Location = dto.Ubicacion;
        entity.Description = dto.Descripcion;
        entity.Status = string.IsNullOrWhiteSpace(dto.Status) ? entity.Status : dto.Status;
        entity.FosterHomeId = dto.FosterHomeId;
        entity.VolunteerId = dto.VolunteerId;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = string.IsNullOrWhiteSpace(dto.ModifiedBy) ? "api" : dto.ModifiedBy;

        await _context.SaveChangesAsync();

        return await GetById(id);
    }

    public async Task<bool> Close(long id)
    {
        var entity = await _context.RescueRecords.FindAsync(id);
        if (entity == null) return false;

        entity.Status = "Cerrado";
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = "api";
        await _context.SaveChangesAsync();

        return true;
    }
}
