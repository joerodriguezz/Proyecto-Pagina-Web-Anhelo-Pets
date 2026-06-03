namespace AnheloPets.API.Models;

public class Animal
{
    public long AnimalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Species { get; set; } = string.Empty;

    public string Breed { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Status { get; set; } = "Disponible";
}
