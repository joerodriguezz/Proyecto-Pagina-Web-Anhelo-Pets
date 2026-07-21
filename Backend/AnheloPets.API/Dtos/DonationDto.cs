namespace AnheloPets.API.DTOs;

public class DonationDto
{
    public long DonationId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly DonatedAt { get; set; }
    public string? Message { get; set; }
    public string ProofFile { get; set; } = string.Empty;
    public string ValidationStatus { get; set; } = "Pendiente";
    public DateTime? CreatedAt { get; set; }
}

public class SubmitDonationDto
{
    public string DonorName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly DonatedAt { get; set; }
    public string? Message { get; set; }
    public string ProofFile { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "public";
}

public class UpdateDonationStatusDto
{
    // action: 'Aprobar' | 'Rechazar'
    public string Action { get; set; } = string.Empty;
    public string? ValidationNotes { get; set; }
    public string ModifiedBy { get; set; } = "admin";
}
