using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Repository;

public class DonationRepository
{
    private static readonly HashSet<string> AllowedActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Aprobar", "Rechazar"
    };

    private readonly AnheloPetsDbContext _context;

    public DonationRepository(AnheloPetsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DonationDto>> GetAllAsync()
    {
        var donations = await _context.Donations
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return donations.Select(ToDto);
    }

    public async Task<DonationDto?> GetByIdAsync(long id)
    {
        var donation = await _context.Donations.FindAsync(id);
        return donation == null ? null : ToDto(donation);
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
            ProofFile = dto.ProofFile,
            ValidationStatus = "Pendiente",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "public" : dto.CreatedBy
        };

        _context.Donations.Add(donation);
        await _context.SaveChangesAsync();

        return ToDto(donation);
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

        return ToDto(donation);
    }

    private static DonationDto ToDto(Donation d) => new()
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
        ProofFile = d.ProofFile,
        ValidationStatus = d.ValidationStatus,
        CreatedAt = d.CreatedAt
    };
}
