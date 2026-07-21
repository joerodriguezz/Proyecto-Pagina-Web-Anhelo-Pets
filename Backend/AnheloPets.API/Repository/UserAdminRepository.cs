using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Repository;

public class UserAdminRepository
{
    private readonly AnheloPetsDbContext _context;

    public UserAdminRepository(AnheloPetsDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserAdminDto>> GetAll()
    {
        var users = await (
            from u in _context.Users
            join p in _context.UserProfiles on u.UserId equals p.UserId into profiles
            from p in profiles.DefaultIfEmpty()
            join c in _context.UserContacts on u.UserId equals c.UserId into contacts
            from c in contacts.DefaultIfEmpty()
            join v in _context.Volunteers on u.UserId equals v.UserId into volunteers
            from v in volunteers.DefaultIfEmpty()
            orderby p.FirstName, p.LastName
            select new UserAdminDto
            {
                UserId = u.UserId!,
                Username = u.Username,
                FullName = (p.FirstName + " " + p.LastName).Trim(),
                Email = c.Email,
                PhonePrimary = c.PhonePrimary,
                PhoneSecondary = c.PhoneSecondary,
                NationalId = p.NationalityId,
                Nationality = p.Nationality,
                City = c.City,
                Town = c.Town,
                AddressLine = c.AddressLine,
                Active = u.Active,
                CreatedAt = u.CreatedAt,
                IsVolunteer = v != null,
                VolunteerType = v != null ? v.VolunteerType : null,
                VolunteerValidationStatus = v != null ? v.ValidationStatus : null,
            })
            .ToListAsync();

        return await WithRoles(users);
    }

    public async Task<UserAdminDto?> GetById(string userId)
    {
        var all = await GetAll();
        return all.FirstOrDefault(u => u.UserId == userId);
    }

    public async Task<bool> UserExists(string userId)
    {
        return await _context.Users.AnyAsync(u => u.UserId == userId);
    }

    public async Task<UserAdminDto?> UpdateStatus(string userId, bool active, string modifiedBy)
    {
        var entity = await _context.Users.FindAsync(userId);
        if (entity == null) return null;

        entity.Active = active;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = string.IsNullOrWhiteSpace(modifiedBy) ? "admin" : modifiedBy;

        await _context.SaveChangesAsync();

        return await GetById(userId);
    }

    /// <summary>
    /// Reemplaza el conjunto completo de roles del usuario por roleIds.
    /// Se calcula la diferencia (a quitar / a agregar) en vez de borrar-todo-e-insertar
    /// para no perder auditoría de las filas que no cambian.
    /// </summary>
    public async Task<UserAdminDto?> SetRoles(string userId, long[] roleIds, string modifiedBy)
    {
        if (!await UserExists(userId)) return null;

        var wantedIds = roleIds.Distinct().ToHashSet();
        var validIds = await _context.Roles
            .Where(r => wantedIds.Contains(r.RoleId))
            .Select(r => r.RoleId)
            .ToListAsync();

        var invalid = wantedIds.Except(validIds).ToList();
        if (invalid.Count > 0)
            throw new ApiException($"Rol inexistente: {string.Join(", ", invalid)}.", 400);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var current = await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
        var currentIds = current.Select(ur => ur.RoleId).ToHashSet();

        var toRemove = current.Where(ur => !wantedIds.Contains(ur.RoleId)).ToList();
        var toAdd = wantedIds.Except(currentIds).Select(roleId => new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = string.IsNullOrWhiteSpace(modifiedBy) ? "admin" : modifiedBy,
        });

        _context.UserRoles.RemoveRange(toRemove);
        _context.UserRoles.AddRange(toAdd);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return await GetById(userId);
    }

    /// <summary>
    /// Los roles no vienen en el join principal (evita duplicar filas de usuario
    /// por cada rol); se resuelven en una consulta aparte y se agrupan en memoria,
    /// igual que HealthRecordRepository.WithVeterinarianNames.
    /// </summary>
    private async Task<List<UserAdminDto>> WithRoles(List<UserAdminDto> users)
    {
        if (users.Count == 0) return users;

        var ids = users.Select(u => u.UserId).ToList();

        var rolesByUser = await (
            from ur in _context.UserRoles
            join r in _context.Roles on ur.RoleId equals r.RoleId
            where ids.Contains(ur.UserId)
            select new { ur.UserId, Role = r })
            .ToListAsync();

        var grouped = rolesByUser
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => new RoleDto
            {
                RoleId = x.Role.RoleId,
                RoleName = x.Role.RoleName,
                RoleAccess = x.Role.RoleAccess,
                Description = x.Role.Description,
            }).ToList());

        foreach (var user in users)
        {
            user.Roles = grouped.GetValueOrDefault(user.UserId, new List<RoleDto>());
        }

        return users;
    }
}
