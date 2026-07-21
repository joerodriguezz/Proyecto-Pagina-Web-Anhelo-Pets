using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Repository;

public class AnimalMedicalRecordRepository
{
    private readonly AnheloPetsDbContext _context;

    public AnimalMedicalRecordRepository(AnheloPetsDbContext context)
    {
        _context = context;
    }

    public async Task<List<AnimalMedicalRecordDto>> GetByAnimal(string animalId)
    {
        var records = await _context.AnimalMedicalRecords
            .Where(r => r.AnimalId == animalId)
            .OrderByDescending(r => r.VisitDate)
            .Select(r => MapToDto(r))
            .ToListAsync();

        return await WithVeterinarianNames(records);
    }

    public async Task<List<AnimalMedicalRecordDto>> GetAll()
    {
        var records = await _context.AnimalMedicalRecords
            .OrderByDescending(r => r.VisitDate)
            .Select(r => MapToDto(r))
            .ToListAsync();

        return await WithVeterinarianNames(records);
    }

    public async Task<AnimalMedicalRecordDto?> GetById(long id)
    {
        var record = await _context.AnimalMedicalRecords
            .Where(r => r.AnimalMedicalRecordId == id)
            .Select(r => MapToDto(r))
            .FirstOrDefaultAsync();

        if (record == null) return null;

        var withNames = await WithVeterinarianNames([record]);
        return withNames[0];
    }

    public async Task<AnimalMedicalRecordDto> Create(AnimalMedicalRecordDto dto)
    {
        var entity = new AnimalMedicalRecord
        {
            AnimalId = dto.AnimalId,
            VeterinarianId = dto.VeterinarianId,
            Diagnosis = dto.Diagnosis,
            Treatment = dto.Treatment,
            Notes = dto.Notes,
            VisitDate = dto.VisitDate,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "api" : dto.CreatedBy,
        };

        _context.AnimalMedicalRecords.Add(entity);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            // Violación de FK contra animals o veterinarians
            throw new ApiException(
                "No se pudo guardar el expediente: la mascota o el veterinario indicados no existen.",
                e,
                400);
        }

        return (await GetById(entity.AnimalMedicalRecordId))!;
    }

    public async Task<bool> Delete(long id)
    {
        var entity = await _context.AnimalMedicalRecords.FindAsync(id);
        if (entity == null) return false;

        _context.AnimalMedicalRecords.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// El nombre del veterinario no vive en veterinarians: hay que recorrer
    /// veterinarians -> volunteers -> user_profiles. Se resuelve con una sola
    /// consulta y se cruza en memoria para no repetir el join por cada fila.
    /// </summary>
    private async Task<List<AnimalMedicalRecordDto>> WithVeterinarianNames(List<AnimalMedicalRecordDto> records)
    {
        if (records.Count == 0) return records;

        var ids = records.Select(r => r.VeterinarianId).Distinct().ToList();

        var names = await (
            from v in _context.Veterinarians
            join vol in _context.Volunteers on v.VolunteerId equals vol.VolunteerId
            join p in _context.UserProfiles on vol.UserId equals p.UserId
            where ids.Contains(v.VeterinarianId!)
            select new { Id = v.VeterinarianId!, Name = p.FirstName + " " + p.LastName })
            .ToDictionaryAsync(x => x.Id, x => x.Name);

        foreach (var record in records)
        {
            record.VeterinarianName = names.GetValueOrDefault(record.VeterinarianId);
        }

        return records;
    }

    private static AnimalMedicalRecordDto MapToDto(AnimalMedicalRecord r) => new()
    {
        AnimalMedicalRecordId = r.AnimalMedicalRecordId,
        AnimalId = r.AnimalId,
        VeterinarianId = r.VeterinarianId,
        Diagnosis = r.Diagnosis,
        Treatment = r.Treatment,
        Notes = r.Notes,
        VisitDate = r.VisitDate,
    };
}
