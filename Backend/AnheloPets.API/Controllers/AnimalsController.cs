using Microsoft.AspNetCore.Mvc;
using AnheloPets.API.DTOs;
using AnheloPets.API.Services;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly IAnimalService _animalService;

    public AnimalsController(IAnimalService animalService)
    {
        _animalService = animalService;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? species, [FromQuery] string? status = "Disponible", [FromQuery] string? search = null)
    {
        return Ok();
    }

    [HttpGet("{id}")]
    public IActionResult GetById(long id)
    {
        var animal = _animalService.GetById(id);

        if (animal == null)
            return NotFound();

        return Ok(animal);
    }

    [HttpPost]
    public IActionResult Create([FromBody] AnimalDto animal)
    {
        if (animal == null) return BadRequest();

        var created = _animalService.Create(animal);
        return CreatedAtAction(nameof(GetById), new { id = created.Result }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(long id, [FromBody] AnimalDto animal)
    {
        if (animal == null) return BadRequest();

        var updated = _animalService.Update(id, animal);
        if (updated == null) return NotFound();

        return Ok(updated);
    }
}
