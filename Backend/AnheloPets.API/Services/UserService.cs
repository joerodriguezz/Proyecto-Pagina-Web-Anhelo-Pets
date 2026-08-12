using System.Security.Cryptography;
using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Services;

public class UserService : IUserService
{
    private const int HashIterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private static readonly string[] AllowedPhotoTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxPhotoBytes = 5 * 1024 * 1024;

    private readonly AuthRepository _authRepository;
    private readonly IJwtService _jwtService;
    private readonly ISupabaseStorageService _storageService;

    private readonly AnheloPetsDbContext _dbContext;

    public UserService(AnheloPetsDbContext dbContext, AuthRepository authRepository, IJwtService jwtService, ISupabaseStorageService storageService)
    {
        _dbContext = dbContext;
        _authRepository = authRepository;
        _jwtService = jwtService;
        _storageService = storageService;
    }

    public async Task<AuthResponseDto> Register(RegisterUserDto request)
    {
        request.Username = request.Email.Split("@")[0];
        var nameParts = request.FirstName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        request.FirstName = nameParts.Length > 0 ? nameParts[0] : request.FirstName;
        request.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
        request.Password = HashPassword(request.Password);

        var created = await _authRepository.Register(request);
        var enriched = await GetAuthUser(created.Username) ?? created;

        return BuildAuthResponse(enriched);
    }


    public async Task<AuthResponseDto> Login(LoginDtoRequest request)
    {
        AuthUserDto authUser;
        try
        {
            authUser = await _authRepository.Login(request);
        }
        catch (NotFoundException)
        {
            throw new UnauthorizedException("Correo o contraseña incorrectos");
        }

        if (!VerifyPassword(request.Password, authUser.Password))
        {
            throw new UnauthorizedException("Correo o contraseña incorrectos");
        }

        var enriched = await GetAuthUser(authUser.Email) ?? authUser;

        return BuildAuthResponse(enriched);
    }

    public async Task<AuthResponseDto?> GetCurrentUser(string username)
    {
        var enriched = await GetAuthUser(username);
        return enriched == null ? null : BuildAuthResponse(enriched);
    }

    public async Task ResetPasswordByEmail(ResetPasswordDto request)
    {
        // fn_update_password_hash espera user_id como bigint, pero user_id es
        // text ("USR-001") desde generate_user_id() — la función SQL quedó
        // escrita para un esquema de IDs viejo. Se actualiza vía EF Core
        // directamente en vez de propagar ese mismatch a código nuevo.
        var contact = await _dbContext.UserContacts.FirstOrDefaultAsync(c => c.Email == request.Email)
            ?? throw new NotFoundException("No existe una cuenta con este correo.");

        var user = await _dbContext.Users.FindAsync(contact.UserId)
            ?? throw new NotFoundException("No existe una cuenta con este correo.");

        user.PasswordHash = HashPassword(request.NewPassword);
        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = "password-reset";
        await _dbContext.SaveChangesAsync();
    }

    public async Task<AuthResponseDto?> UploadProfilePhoto(string userId, IFormFile file)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (user == null || profile == null)
        {
            return null;
        }

        if (file == null || file.Length == 0)
        {
            throw new BadRequestException("El archivo es obligatorio.");
        }

        if (!AllowedPhotoTypes.Contains(file.ContentType))
        {
            throw new BadRequestException("Solo se permiten imágenes JPG, PNG o WEBP.");
        }

        if (file.Length > MaxPhotoBytes)
        {
            throw new BadRequestException("La imagen no puede superar 5MB.");
        }

        var path = $"profiles/{userId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string publicUrl;
        using (var stream = file.OpenReadStream())
        {
            publicUrl = await _storageService.UploadPublicAsync("public-media", path, stream, file.ContentType);
        }

        profile.PhotoUrl = publicUrl;
        profile.ModifiedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return await GetCurrentUser(user.Username);
    }

    public async Task<AuthResponseDto?> DeleteProfilePhoto(string userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (user == null || profile == null)
        {
            return null;
        }

        profile.PhotoUrl = null;
        profile.ModifiedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return await GetCurrentUser(user.Username);
    }

    private AuthResponseDto BuildAuthResponse(AuthUserDto user)
    {
        return new AuthResponseDto
        {
            Token = _jwtService.GenerateToken(user),
            UserId = user.UserId ?? string.Empty,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = user.Roles,
            IsVolunteer = user.IsVolunteer ?? false,
            VolunteerActive = user.VolunteerActive ?? false,
            VolunteerValidationStatus = user.VolunteerValidationStatus,
            PhotoUrl = user.PhotoUrl,
            NationalId = user.NationalId,
            PhonePrimary = user.PhonePrimary
        };
    }

    private async Task<AuthUserDto?> GetAuthUser(string usernameOrEmail)
    {
        var key = usernameOrEmail.Trim().ToLower();

        var row = await (
            from u in _dbContext.Users
            join up in _dbContext.UserProfiles on u.UserId equals up.UserId
            join uc in _dbContext.UserContacts on u.UserId equals uc.UserId
            where u.Username.ToLower() == key || uc.Email.ToLower() == key
            select new { u, up, uc }
        ).FirstOrDefaultAsync();

        if (row == null)
        {
            return null;
        }

        var volunteer = await _dbContext.Volunteers.FirstOrDefaultAsync(v => v.UserId == row.u.UserId);

        var roles = await (
            from ur in _dbContext.UserRoles
            join r in _dbContext.Roles on ur.RoleId equals r.RoleId
            where ur.UserId == row.u.UserId
            select (string?)r.RoleName
        ).ToArrayAsync();

        return new AuthUserDto
        {
            UserId = row.u.UserId,
            Username = row.u.Username,
            Email = row.uc.Email,
            Password = row.u.PasswordHash,
            FirstName = row.up.FirstName,
            LastName = row.up.LastName,
            IsVolunteer = volunteer != null,
            VolunteerActive = volunteer?.Active ?? false,
            VolunteerValidationStatus = volunteer?.ValidationStatus,
            Roles = roles,
            PhotoUrl = row.up.PhotoUrl,
            NationalId = row.up.NationalityId,
            PhonePrimary = row.uc.PhonePrimary
        };
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, HashIterations, HashAlgorithmName.SHA256, KeySize);

        return $"pbkdf2${HashIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('$');
        if (parts.Length != 4 || parts[0] != "pbkdf2" || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expectedHash = Convert.FromBase64String(parts[3]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
