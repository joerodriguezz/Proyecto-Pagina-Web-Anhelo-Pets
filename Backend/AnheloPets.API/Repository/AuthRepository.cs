using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Models;
using AnheloPets.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Repository;

public class AuthRepository

{
    private readonly AnheloPetsDbContext _context;
    
    public AuthRepository(AnheloPetsDbContext context)
    {
        _context = context;
    }
    
    public async Task<AuthUserDto> Register(RegisterUserDto dto)
    {
        // Se guarda en la tabla de users
        User user = new User
        {
            Username = dto.Username,
            PasswordHash = dto.Password
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        // Ahora se asignan las relaciones y se guardan junto con el usuario
        UserProfile profile = new UserProfile
        {
           UserId = user.UserId ?? string.Empty,
           FirstName = dto.FirstName ?? string.Empty,
           LastName = dto.LastName ?? string.Empty,
           Nationality = dto.Nationality ?? string.Empty,
           NationalityId = dto.NationalId ?? string.Empty
        };
        
        UserContacts contacts = new UserContacts
        {
           UserId = user.UserId ?? string.Empty,
           Email = dto.Email ?? string.Empty,
           PhonePrimary = dto.PhonePrimary ?? string.Empty
        };
        
        _context.UserProfiles.Add(profile);
        _context.UserContacts.Add(contacts);
        await _context.SaveChangesAsync();
        
        return new AuthUserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = contacts.Email,
            FirstName = profile.FirstName,
            LastName = profile.LastName
        };
    }


    public async Task<AuthUserDto> Login(LoginDtoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
           throw new BadRequestException("Email no puede estar vacío");
        }
        
        var userContacts = await _context.UserContacts
           .Include(c => c.User)
           .FirstOrDefaultAsync(c => c.Email.Equals(request.Email));
        
        if (userContacts?.User == null)
        {
           throw new NotFoundException("Usuario no encontrado");
        }
        
        // Obtener el perfil del usuario
        var userProfile = await _context.UserProfiles
           .FirstOrDefaultAsync(p => p.UserId.Equals(userContacts.User.UserId));
        
        return new AuthUserDto
        {
           UserId = userContacts.User.UserId,
           Username = userContacts.User.Username,
           Email = userContacts.Email,
           FirstName = userProfile?.FirstName ?? string.Empty,
           LastName = userProfile?.LastName ?? string.Empty,
           Password = userContacts.User.PasswordHash
        };
    }
}