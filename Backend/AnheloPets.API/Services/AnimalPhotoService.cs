using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Services;

public class AnimalPhotoService : IAnimalPhotoService
{
    private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxPhotoBytes = 5 * 1024 * 1024;

    private readonly AnheloPetsDbContext _context;
    private readonly ISupabaseStorageService _storageService;

    public AnimalPhotoService(AnheloPetsDbContext context, ISupabaseStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<List<AnimalPhotoDto>> GetByAnimalId(string animalId)
    {
        return await _context.AnimalPhotos
            .Where(p => p.AnimalId == animalId)
            .OrderByDescending(p => p.IsPrimary)
            .ThenBy(p => p.DisplayOrder)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<AnimalPhotoDto> Upload(string animalId, IFormFile file, bool isPrimary)
    {
        var animalExists = await _context.Animals.AnyAsync(a => a.AnimalId == animalId);
        if (!animalExists)
        {
            throw new NotFoundException($"No se encontró la mascota con ID {animalId}.");
        }

        if (file == null || file.Length == 0)
        {
            throw new BadRequestException("El archivo es obligatorio.");
        }

        if (!AllowedTypes.Contains(file.ContentType))
        {
            throw new BadRequestException("Solo se permiten imágenes JPG, PNG o WEBP.");
        }

        if (file.Length > MaxPhotoBytes)
        {
            throw new BadRequestException("La imagen no puede superar 5MB.");
        }

        var noPhotosYet = !await _context.AnimalPhotos.AnyAsync(p => p.AnimalId == animalId);
        var makePrimary = isPrimary || noPhotosYet;

        var path = $"animals/{animalId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string publicUrl;
        using (var stream = file.OpenReadStream())
        {
            publicUrl = await _storageService.UploadPublicAsync("public-media", path, stream, file.ContentType);
        }

        if (makePrimary)
        {
            await ClearPrimary(animalId);
        }

        var maxOrder = await _context.AnimalPhotos
            .Where(p => p.AnimalId == animalId)
            .Select(p => (int?)p.DisplayOrder)
            .MaxAsync() ?? -1;

        var photo = new AnimalPhoto
        {
            AnimalId = animalId,
            PhotoUrl = publicUrl,
            IsPrimary = makePrimary,
            DisplayOrder = maxOrder + 1,
            CreatedAt = DateTime.UtcNow,
        };

        _context.AnimalPhotos.Add(photo);
        await _context.SaveChangesAsync();

        return ToDto(photo);
    }

    public async Task<AnimalPhotoDto> SetPrimary(long photoId)
    {
        var photo = await _context.AnimalPhotos.FindAsync(photoId)
            ?? throw new NotFoundException($"No se encontró la foto con ID {photoId}.");

        await ClearPrimary(photo.AnimalId);

        photo.IsPrimary = true;
        photo.ModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ToDto(photo);
    }

    public async Task Delete(long photoId)
    {
        var photo = await _context.AnimalPhotos.FindAsync(photoId)
            ?? throw new NotFoundException($"No se encontró la foto con ID {photoId}.");

        _context.AnimalPhotos.Remove(photo);
        await _context.SaveChangesAsync();

        // Best-effort: si falla el borrado en Supabase, el registro en BD ya se quitó igual.
        var path = ExtractPath(photo.PhotoUrl);
        if (path != null)
        {
            await _storageService.DeleteAsync("public-media", path);
        }
    }

    private async Task ClearPrimary(string animalId)
    {
        var currentPrimary = await _context.AnimalPhotos
            .Where(p => p.AnimalId == animalId && p.IsPrimary)
            .ToListAsync();

        foreach (var p in currentPrimary)
        {
            p.IsPrimary = false;
        }
    }

    private static string? ExtractPath(string publicUrl)
    {
        const string marker = "/public-media/";
        var idx = publicUrl.IndexOf(marker, StringComparison.Ordinal);
        return idx < 0 ? null : publicUrl[(idx + marker.Length)..];
    }

    private static AnimalPhotoDto ToDto(AnimalPhoto p) => new()
    {
        AnimalPhotoId = p.AnimalPhotoId,
        AnimalId = p.AnimalId,
        PhotoUrl = p.PhotoUrl,
        Description = p.Description,
        IsPrimary = p.IsPrimary,
        DisplayOrder = p.DisplayOrder,
    };
}
