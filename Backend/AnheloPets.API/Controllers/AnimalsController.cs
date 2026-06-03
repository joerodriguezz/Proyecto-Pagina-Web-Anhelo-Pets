using Microsoft.AspNetCore.Mvc;
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
    public IActionResult GetAll()
    {
        return Ok(_animalService.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult GetById(long id)
    {
        var animal = _animalService.GetById(id);

        if (animal == null)
            return NotFound();

        return Ok(animal);
    }
}
