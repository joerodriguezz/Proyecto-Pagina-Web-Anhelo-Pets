using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Models;
using AnheloPets.API.Services;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Repository;

public class DonationRepository
{
    private static readonly HashSet<string> AllowedActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Aprobar", "Rechazar"
    };

    private const string PrivateBucket = "donation-proofs";

    private static readonly Dictionary<string, string> MimeToExtension = new()
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["application/pdf"] = ".pdf",
    };

    private readonly AnheloPetsDbContext _context;
    private readonly ISupabaseStorageService _storageService;

    public DonationRepository(AnheloPetsDbContext context, ISupabaseStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<IEnumerable<DonationDto>> GetAllAsync()
    {
        var donations = await _context.Donations
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var dtos = new List<DonationDto>(donations.Count);
        foreach (var donation in donations)
        {
            dtos.Add(await ToDtoAsync(donation));
        }

        return dtos;
    }

    public async Task<DonationDto?> GetByIdAsync(long id)
    {
        var donation = await _context.Donations.FindAsync(id);
        return donation == null ? null : await ToDtoAsync(donation);
    }

    public async Task<DonationDto> CreateAsync(SubmitDonationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DonorName))
        {
            throw new BadRequestException("El nombre es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new BadRequestException("El correo es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(dto.Phone))
        {
            throw new BadRequestException("El teléfono es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(dto.Method))
        {
            throw new BadRequestException("El método de donación es obligatorio.");
        }

        if (dto.Currency != "CRC" && dto.Currency != "USD")
        {
            throw new BadRequestException("La moneda debe ser CRC o USD.");
        }

        if (dto.Amount <= 0)
        {
            throw new BadRequestException("El monto debe ser mayor a cero.");
        }

        if (string.IsNullOrWhiteSpace(dto.ProofFile))
        {
            throw new BadRequestException("El comprobante es obligatorio.");
        }

        var proofPath = await UploadProofAsync(dto.ProofFile);

        var donation = new Donation
        {
            DonorName = dto.DonorName.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone.Trim(),
            Method = dto.Method,
            Currency = dto.Currency,
            Amount = dto.Amount,
            DonatedAt = dto.DonatedAt,
            Message = string.IsNullOrWhiteSpace(dto.Message) ? null : dto.Message.Trim(),
            ProofFile = proofPath,
            ValidationStatus = "Pendiente",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "public" : dto.CreatedBy
        };

        _context.Donations.Add(donation);
        await _context.SaveChangesAsync();

        return await ToDtoAsync(donation);
    }

    public async Task<DonationDto> UpdateStatusAsync(long id, UpdateDonationStatusDto dto)
    {
        if (!AllowedActions.Contains(dto.Action))
        {
            throw new BadRequestException($"Acción inválida: {dto.Action}.");
        }

        var donation = await _context.Donations.FindAsync(id)
            ?? throw new NotFoundException("Donación no encontrada.");

        donation.ValidationStatus = dto.Action.Equals("Aprobar", StringComparison.OrdinalIgnoreCase)
            ? "Aprobada"
            : "Rechazada";
        donation.ValidationNotes = dto.ValidationNotes;
        donation.ValidatedAt = DateTime.UtcNow;
        donation.ModifiedAt = DateTime.UtcNow;
        donation.ModifiedBy = string.IsNullOrWhiteSpace(dto.ModifiedBy) ? "admin" : dto.ModifiedBy;

        await _context.SaveChangesAsync();

        return await ToDtoAsync(donation);
    }

    /// <summary>Decodifica el data URL (data:&lt;mime&gt;;base64,&lt;payload&gt;) que manda el
    /// frontend y sube los bytes al bucket privado. Devuelve el path guardado, no una URL.</summary>
    private async Task<string> UploadProofAsync(string dataUrl)
    {
        var commaIndex = dataUrl.IndexOf(',');
        if (!dataUrl.StartsWith("data:") || commaIndex < 0)
        {
            throw new BadRequestException("El comprobante tiene un formato inválido.");
        }

        var header = dataUrl[5..commaIndex]; // "<mime>;base64"
        var mime = header.Split(';')[0];

        if (!MimeToExtension.TryGetValue(mime, out var extension))
        {
            throw new BadRequestException("Solo se permiten comprobantes JPG, PNG, WEBP o PDF.");
        }

        var base64Payload = dataUrl[(commaIndex + 1)..];
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64Payload);
        }
        catch (FormatException)
        {
            throw new BadRequestException("El comprobante tiene un formato inválido.");
        }

        if (bytes.Length > 10 * 1024 * 1024)
        {
            throw new BadRequestException("El comprobante no puede superar 10MB.");
        }

        var path = $"donations/{Guid.NewGuid()}{extension}";
        using var stream = new MemoryStream(bytes);
        return await _storageService.UploadPrivateAsync(PrivateBucket, path, stream, mime);
    }

    private async Task<DonationDto> ToDtoAsync(Donation d)
    {
        string? signedUrl;
        try
        {
            signedUrl = await _storageService.CreateSignedUrlAsync(PrivateBucket, d.ProofFile);
        }
        catch
        {
            signedUrl = null;
        }

        return new DonationDto
        {
            DonationId = d.DonationId,
            DonorName = d.DonorName,
            Email = d.Email,
            Phone = d.Phone,
            Method = d.Method,
            Currency = d.Currency,
            Amount = d.Amount,
            DonatedAt = d.DonatedAt,
            Message = d.Message,
            ProofFile = signedUrl ?? d.ProofFile,
            ValidationStatus = d.ValidationStatus,
            CreatedAt = d.CreatedAt
        };
    }
}
