using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Repository;

public class RoleRepository
{
    private readonly AnheloPetsDbContext _context;

    public RoleRepository(AnheloPetsDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleDto>> GetAll()
    {
        return await _context.Roles
            .OrderBy(r => r.RoleName)
            .Select(r => new RoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                RoleAccess = r.RoleAccess,
                Description = r.Description,
                UserCount = _context.UserRoles.Count(ur => ur.RoleId == r.RoleId)
            })
            .ToListAsync();
    }

    public async Task<RoleDto?> GetById(long id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return null;

        return await MapToDto(role);
    }

    public async Task<RoleDto> Create(CreateRoleDto dto)
    {
        var entity = new Role
        {
            RoleName = dto.RoleName.Trim(),
            RoleAccess = dto.RoleAccess.Trim(),
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "admin" : dto.CreatedBy,
        };

        _context.Roles.Add(entity);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            throw new ApiException("Ya existe un rol con ese nombre.", e, 409);
        }

        return (await MapToDto(entity))!;
    }

    public async Task<RoleDto?> Update(long id, UpdateRoleDto dto)
    {
        var entity = await _context.Roles.FindAsync(id);
        if (entity == null) return null;

        entity.RoleName = dto.RoleName.Trim();
        entity.RoleAccess = dto.RoleAccess.Trim();
        entity.Description = dto.Description;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = string.IsNullOrWhiteSpace(dto.ModifiedBy) ? "admin" : dto.ModifiedBy;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            throw new ApiException("Ya existe un rol con ese nombre.", e, 409);
        }

        return await MapToDto(entity);
    }

    public async Task<bool> Delete(long id)
    {
        var entity = await _context.Roles.FindAsync(id);
        if (entity == null) return false;

        _context.Roles.Remove(entity);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            // fk_user_roles_role_id ... ON DELETE RESTRICT
            throw new ApiException("No se puede eliminar: hay usuarios con este rol asignado.", e, 409);
        }

        return true;
    }

    private async Task<RoleDto> MapToDto(Role role) => new()
    {
        RoleId = role.RoleId,
        RoleName = role.RoleName,
        RoleAccess = role.RoleAccess,
        Description = role.Description,
        UserCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == role.RoleId)
    };
}
