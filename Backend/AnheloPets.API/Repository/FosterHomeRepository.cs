using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Repository;

public class FosterHomeRepository
{
    private readonly AnheloPetsDbContext _context;

    public FosterHomeRepository(AnheloPetsDbContext context)
    {
        _context = context;
    }

    public async Task<List<FosterHomeDto>> GetAll()
    {
        return await _context.FosterHomes
            .OrderBy(f => f.Name)
            .Select(f => MapToDto(f))
            .ToListAsync();
    }

    public async Task<FosterHomeDto?> GetById(string id)
    {
        var entity = await _context.FosterHomes.FindAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<FosterHomeDto> Create(FosterHomeDto dto)
    {
        var entity = new FosterHome
        {
            VolunteerId = dto.VolunteerId,
            Name = dto.Name,
            Address = dto.Address,
            Phone = dto.Phone,
            Responsible = dto.Responsible,
            Capacity = dto.Capacity,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "api" : dto.CreatedBy,
        };

        _context.FosterHomes.Add(entity);
        await _context.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<FosterHomeDto?> Update(string id, FosterHomeDto dto)
    {
        var entity = await _context.FosterHomes.FindAsync(id);
        if (entity == null) return null;

        entity.VolunteerId = dto.VolunteerId;
        entity.Name = dto.Name;
        entity.Address = dto.Address;
        entity.Phone = dto.Phone;
        entity.Responsible = dto.Responsible;
        entity.Capacity = dto.Capacity;
        entity.Active = dto.Active;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = string.IsNullOrWhiteSpace(dto.ModifiedBy) ? "api" : dto.ModifiedBy;

        await _context.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<bool> Deactivate(string id)
    {
        var entity = await _context.FosterHomes.FindAsync(id);
        if (entity == null) return false;

        entity.Active = false;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = "api";
        await _context.SaveChangesAsync();

        return true;
    }

    private static FosterHomeDto MapToDto(FosterHome f) => new()
    {
        FosterHomeId = f.FosterHomeId,
        VolunteerId = f.VolunteerId,
        Name = f.Name,
        Address = f.Address,
        Phone = f.Phone,
        Responsible = f.Responsible,
        Capacity = f.Capacity,
        Active = f.Active,
    };
}
