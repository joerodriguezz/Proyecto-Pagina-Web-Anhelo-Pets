using System.Security.Cryptography;
using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Repository;

public class VeterinarianRepository
{
    private const string VolunteerTypeVeterinaria = "Veterinaria";

    private readonly AnheloPetsDbContext _context;

    public VeterinarianRepository(AnheloPetsDbContext context)
    {
        _context = context;
    }

    public async Task<List<VeterinarianDto>> GetAll()
    {
        return await BaseQuery()
            .OrderBy(v => v.FirstName)
            .ThenBy(v => v.LastName)
            .ToListAsync();
    }

    public async Task<VeterinarianDto?> GetById(string id)
    {
        return await BaseQuery().FirstOrDefaultAsync(v => v.VeterinarianId == id);
    }

    /// <summary>
    /// Alta en cascada. veterinarians exige un volunteer_id existente y
    /// volunteers un user_id, así que se crean las cuatro filas en una
    /// transacción: si algo falla no queda ningún usuario huérfano.
    /// </summary>
    public async Task<VeterinarianDto> Create(CreateVeterinarianDto dto)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = new User
            {
                Username = await BuildUniqueUsername(dto),
                // El veterinario se da de alta desde el panel, no se registra por sí
                // mismo: se guarda un hash aleatorio inutilizable para que la cuenta
                // no sea accesible hasta que se restablezca la contraseña.
                PasswordHash = BuildUnusablePasswordHash(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var profile = new UserProfile
            {
                UserId = user.UserId!,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                NationalityId = dto.NationalId?.Trim() ?? string.Empty,
                Nationality = string.IsNullOrWhiteSpace(dto.Nationality) ? "Costarricense" : dto.Nationality.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
            };

            var volunteer = new Volunteer
            {
                UserId = user.UserId!,
                Active = true,
                NationalId = dto.NationalId?.Trim(),
                VolunteerType = VolunteerTypeVeterinaria,
                // Alta administrativa: nace ya aprobado
                ValidationStatus = "Aprobado",
                ValidatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
            };

            _context.UserProfiles.Add(profile);
            _context.Volunteers.Add(volunteer);
            await _context.SaveChangesAsync();

            var veterinarian = new Veterinarian
            {
                VolunteerId = volunteer.VolunteerId!,
                Specialty = dto.Specialty.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
            };

            _context.Veterinarians.Add(veterinarian);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return (await GetById(veterinarian.VeterinarianId!))!;
        }
        catch (DbUpdateException e)
        {
            await transaction.RollbackAsync();
            throw new ApiException("No se pudo crear el veterinario.", e, 400);
        }
    }

    public async Task<VeterinarianDto?> Update(string id, UpdateVeterinarianDto dto)
    {
        var entity = await _context.Veterinarians.FindAsync(id);
        if (entity == null) return null;

        entity.Specialty = dto.Specialty.Trim();
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = string.IsNullOrWhiteSpace(dto.ModifiedBy) ? "api" : dto.ModifiedBy;

        await _context.SaveChangesAsync();

        return await GetById(id);
    }

    /// <summary>
    /// Baja lógica: se desactiva el voluntario asociado. No se borra la fila de
    /// veterinarians porque animal_medical_records la referencia con ON DELETE RESTRICT.
    /// </summary>
    public async Task<bool> Deactivate(string id)
    {
        var entity = await _context.Veterinarians.FindAsync(id);
        if (entity == null) return false;

        var volunteer = await _context.Volunteers.FindAsync(entity.VolunteerId);
        if (volunteer == null) return false;

        volunteer.Active = false;
        volunteer.ModifiedAt = DateTime.UtcNow;
        volunteer.ModifiedBy = "api";

        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = "api";

        await _context.SaveChangesAsync();
        return true;
    }

    private IQueryable<VeterinarianDto> BaseQuery()
    {
        return from v in _context.Veterinarians
               join vol in _context.Volunteers on v.VolunteerId equals vol.VolunteerId
               join p in _context.UserProfiles on vol.UserId equals p.UserId
               select new VeterinarianDto
               {
                   VeterinarianId = v.VeterinarianId,
                   VolunteerId = v.VolunteerId,
                   Specialty = v.Specialty,
                   FirstName = p.FirstName,
                   LastName = p.LastName,
                   NationalId = vol.NationalId,
                   ValidationStatus = vol.ValidationStatus,
                   Active = vol.Active,
               };
    }

    /// <summary>users.username tiene restricción UNIQUE: se añade sufijo si hace falta.</summary>
    private async Task<string> BuildUniqueUsername(CreateVeterinarianDto dto)
    {
        var seed = !string.IsNullOrWhiteSpace(dto.Email)
            ? dto.Email.Split('@')[0]
            : $"{dto.FirstName}.{dto.LastName}";

        var normalized = new string(seed.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) || c is '.' or '_' or '-' ? c : '.')
            .ToArray());

        if (string.IsNullOrWhiteSpace(normalized)) normalized = "veterinario";
        if (normalized.Length > 90) normalized = normalized[..90];

        var candidate = normalized;
        var suffix = 1;

        while (await _context.Users.AnyAsync(u => u.Username == candidate))
        {
            candidate = $"{normalized}{++suffix}";
        }

        return candidate;
    }

    /// <summary>
    /// Mismo formato que UserService (pbkdf2$iteraciones$salt$hash) pero sobre un
    /// secreto aleatorio que nadie conoce: la cuenta queda sin acceso.
    /// </summary>
    private static string BuildUnusablePasswordHash()
    {
        const int iterations = 100_000;
        var salt = RandomNumberGenerator.GetBytes(16);
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var hash = Rfc2898DeriveBytes.Pbkdf2(secret, salt, iterations, HashAlgorithmName.SHA256, 32);

        return $"pbkdf2${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }
}
