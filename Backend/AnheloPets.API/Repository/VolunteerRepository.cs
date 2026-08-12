using System.Security.Cryptography;
using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Repository;

public class VolunteerRepository
{
    private static readonly HashSet<string> AllowedActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Aprobar", "Rechazar", "Inactivar", "Reactivar"
    };

    private readonly AnheloPetsDbContext _context;

    public VolunteerRepository(AnheloPetsDbContext context)
    {
        _context = context;
    }

    public async Task<List<VolunteerDto>> GetAll()
    {
        return await BaseQuery()
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<VolunteerDto?> GetById(string volunteerId)
    {
        return await BaseQuery().FirstOrDefaultAsync(v => v.VolunteerId == volunteerId);
    }

    public async Task<VolunteerDto?> GetByEmail(string email)
    {
        var normalized = email.Trim().ToLower();
        return await BaseQuery().FirstOrDefaultAsync(v => v.Email != null && v.Email.ToLower() == normalized);
    }

    /// <summary>
    /// Envío del formulario público. Resuelve el usuario por correo (debe existir:
    /// el voluntariado no crea cuentas) y crea o reemplaza su solicitud. Reenviar
    /// el formulario reinicia el proceso de validación a Pendiente, igual que hacía
    /// la función SQL que reemplaza este método.
    /// </summary>
    public async Task<VolunteerDto> Submit(SubmitVolunteerApplicationDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLower();
        var contact = await _context.UserContacts
            .FirstOrDefaultAsync(c => c.Email.ToLower() == normalizedEmail);

        if (contact == null)
            throw new ApiException("No existe una cuenta con este correo. Regístrate primero.", 404);

        var userId = contact.UserId;
        var actor = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "public" : dto.CreatedBy;

        contact.PhonePrimary = dto.PhonePrimary.Trim();
        contact.City = dto.City;
        contact.Town = dto.Town;
        contact.District = dto.District;
        contact.ModifiedAt = DateTime.UtcNow;
        contact.ModifiedBy = actor;

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile != null)
        {
            profile.NationalityId = dto.NationalId.Trim();
            profile.ModifiedAt = DateTime.UtcNow;
            profile.ModifiedBy = actor;
        }

        var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.UserId == userId);

        if (volunteer == null)
        {
            volunteer = new Volunteer
            {
                UserId = userId,
                Active = false,
                NationalId = dto.NationalId.Trim(),
                VolunteerType = dto.VolunteerType.Trim(),
                Motivation = dto.Motivation,
                ApplicationDetails = dto.ApplicationDetails,
                ValidationStatus = "Pendiente",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = actor,
            };
            _context.Volunteers.Add(volunteer);
        }
        else
        {
            volunteer.NationalId = dto.NationalId.Trim();
            volunteer.VolunteerType = dto.VolunteerType.Trim();
            volunteer.Motivation = dto.Motivation;
            volunteer.ApplicationDetails = dto.ApplicationDetails;
            volunteer.ValidationStatus = "Pendiente";
            volunteer.ValidationNotes = null;
            volunteer.ValidatedAt = null;
            volunteer.ValidatedByUserId = null;
            volunteer.Active = false;
            volunteer.ModifiedAt = DateTime.UtcNow;
            volunteer.ModifiedBy = actor;
        }

        await _context.SaveChangesAsync();

        return (await GetById(volunteer.VolunteerId!))!;
    }

    /// <summary>
    /// Alta administrativa en cascada (mismo patrón que VeterinarianRepository.Create):
    /// crea user + user_profile + user_contacts + volunteer (ya aprobado) en una sola
    /// transacción, así no queda una cuenta huérfana sin voluntariado si algo falla a
    /// mitad de camino. A diferencia de Submit(), esta sí crea el usuario.
    /// </summary>
    public async Task<VolunteerDto> CreateApproved(CreateApprovedVolunteerDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLower();
        var emailExists = await _context.UserContacts.AnyAsync(c => c.Email.ToLower() == normalizedEmail);
        if (emailExists)
            throw new ApiException("Ya existe una cuenta con este correo.", 409);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = new User
            {
                Username = await BuildUniqueUsername(dto),
                PasswordHash = HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var profile = new UserProfile
            {
                UserId = user.UserId!,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName?.Trim() ?? string.Empty,
                NationalityId = dto.NationalId.Trim(),
                Nationality = string.IsNullOrWhiteSpace(dto.Nationality) ? "Costarricense" : dto.Nationality.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
            };

            var contacts = new UserContacts
            {
                UserId = user.UserId!,
                Email = dto.Email.Trim(),
                PhonePrimary = dto.PhonePrimary.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
            };

            var volunteer = new Volunteer
            {
                UserId = user.UserId!,
                Active = true,
                NationalId = dto.NationalId.Trim(),
                VolunteerType = dto.VolunteerType.Trim(),
                ValidationStatus = "Aprobado",
                ValidatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy,
            };

            _context.UserProfiles.Add(profile);
            _context.UserContacts.Add(contacts);
            _context.Volunteers.Add(volunteer);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return (await GetById(volunteer.VolunteerId!))!;
        }
        catch (DbUpdateException e)
        {
            await transaction.RollbackAsync();
            throw new ApiException("No se pudo crear el voluntario.", e, 400);
        }
    }

    public async Task<VolunteerDto?> Update(string volunteerId, UpdateVolunteerDto dto)
    {
        var volunteer = await _context.Volunteers.FindAsync(volunteerId);
        if (volunteer == null) return null;

        var actor = string.IsNullOrWhiteSpace(dto.ModifiedBy) ? "admin" : dto.ModifiedBy;

        if (dto.NationalId != null) volunteer.NationalId = dto.NationalId.Trim();
        if (dto.VolunteerType != null) volunteer.VolunteerType = dto.VolunteerType.Trim();
        if (dto.Motivation != null) volunteer.Motivation = dto.Motivation;
        if (dto.ApplicationDetails != null) volunteer.ApplicationDetails = dto.ApplicationDetails;
        volunteer.ModifiedAt = DateTime.UtcNow;
        volunteer.ModifiedBy = actor;

        var contact = await _context.UserContacts.FirstOrDefaultAsync(c => c.UserId == volunteer.UserId);
        if (contact != null)
        {
            if (dto.PhonePrimary != null) contact.PhonePrimary = dto.PhonePrimary.Trim();
            if (dto.City != null) contact.City = dto.City;
            if (dto.Town != null) contact.Town = dto.Town;
            if (dto.District != null) contact.District = dto.District;
            contact.ModifiedAt = DateTime.UtcNow;
            contact.ModifiedBy = actor;
        }

        await _context.SaveChangesAsync();

        return await GetById(volunteerId);
    }

    /// <summary>
    /// Traduce la acción administrativa al par (validation_status, active), igual que
    /// hacía la combinación de fn_validate_volunteer + fn_set_volunteer_active.
    /// </summary>
    public async Task<VolunteerDto?> UpdateStatus(string volunteerId, UpdateVolunteerStatusDto dto)
    {
        if (!AllowedActions.Contains(dto.Action))
            throw new ApiException($"Acción inválida: {dto.Action}.", 400);

        var volunteer = await _context.Volunteers.FindAsync(volunteerId);
        if (volunteer == null) return null;

        switch (dto.Action.ToLowerInvariant())
        {
            case "aprobar":
                volunteer.ValidationStatus = "Aprobado";
                volunteer.Active = true;
                volunteer.ValidatedAt = DateTime.UtcNow;
                break;
            case "rechazar":
                volunteer.ValidationStatus = "Rechazado";
                volunteer.Active = false;
                volunteer.ValidatedAt = DateTime.UtcNow;
                break;
            case "inactivar":
                volunteer.Active = false;
                break;
            case "reactivar":
                volunteer.Active = true;
                break;
        }

        if (dto.ValidationNotes != null) volunteer.ValidationNotes = dto.ValidationNotes;
        volunteer.ModifiedAt = DateTime.UtcNow;
        volunteer.ModifiedBy = string.IsNullOrWhiteSpace(dto.ModifiedBy) ? "admin" : dto.ModifiedBy;

        await _context.SaveChangesAsync();

        return await GetById(volunteerId);
    }

    /// <summary>users.username tiene restricción UNIQUE: se añade sufijo si hace falta.</summary>
    private async Task<string> BuildUniqueUsername(CreateApprovedVolunteerDto dto)
    {
        var seed = dto.Email.Split('@')[0];

        var normalized = new string(seed.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) || c is '.' or '_' or '-' ? c : '.')
            .ToArray());

        if (string.IsNullOrWhiteSpace(normalized)) normalized = "voluntario";
        if (normalized.Length > 90) normalized = normalized[..90];

        var candidate = normalized;
        var suffix = 1;

        while (await _context.Users.AnyAsync(u => u.Username == candidate))
        {
            candidate = $"{normalized}{++suffix}";
        }

        return candidate;
    }

    private static string HashPassword(string password)
    {
        const int iterations = 100_000;
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);

        return $"pbkdf2${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    private IQueryable<VolunteerDto> BaseQuery()
    {
        return from v in _context.Volunteers
               join p in _context.UserProfiles on v.UserId equals p.UserId
               join c in _context.UserContacts on v.UserId equals c.UserId
               select new VolunteerDto
               {
                   VolunteerId = v.VolunteerId!,
                   UserId = v.UserId,
                   FullName = (p.FirstName + " " + p.LastName).Trim(),
                   NationalId = v.NationalId,
                   VolunteerType = v.VolunteerType,
                   Motivation = v.Motivation,
                   ApplicationDetails = v.ApplicationDetails,
                   Email = c.Email,
                   PhonePrimary = c.PhonePrimary,
                   City = c.City,
                   Town = c.Town,
                   District = c.District,
                   Active = v.Active,
                   ValidationStatus = v.ValidationStatus,
                   ValidationNotes = v.ValidationNotes,
                   ValidatedAt = v.ValidatedAt,
                   CreatedAt = v.CreatedAt,
               };
    }
}
