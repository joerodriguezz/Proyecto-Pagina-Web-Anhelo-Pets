namespace AnheloPets.API.DTOs;

public class DonationDto
{
    public long DonationId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DonatedAt { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = string.Empty;
}
