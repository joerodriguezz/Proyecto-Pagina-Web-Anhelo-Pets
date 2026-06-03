namespace AnheloPets.API.DTOs;

public class FosterHomeDto
{
    public long FosterHomeId { get; set; }
    public long? VolunteerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Responsible { get; set; } = string.Empty;
    public int Capacity { get; set; } = 1;
    public bool Active { get; set; } = true;
    public string CreatedBy { get; set; } = "api";
    public string ModifiedBy { get; set; } = "api";
}
