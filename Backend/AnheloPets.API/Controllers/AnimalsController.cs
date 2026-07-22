using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AnheloPets.API.DTOs;
using AnheloPets.API.Services;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly IAnimalService _animalService;
    private readonly IAnimalPhotoService _animalPhotoService;

    public AnimalsController(IAnimalService animalService, IAnimalPhotoService animalPhotoService)
    {
        _animalService = animalService;
        _animalPhotoService = animalPhotoService;
    }

    [HttpGet]
    public IActionResult GetAll(
        [FromQuery] string? species,
        [FromQuery] string? status = "Disponible",
        [FromQuery] string? search = null,
        [FromQuery] string? column = null,
        [FromQuery] string? value = null)
    {
        var animals = _animalService.GetAll(species, status, search, column, value);
        return Ok(animals);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var animal = _animalService.GetById(id);

        if (animal == null)
            return NotFound();

        return Ok(animal);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AnimalDto animal)
    {
        if (animal == null) return BadRequest();

        var created = await _animalService.Create(animal);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] AnimalDto animal)
    {
        if (animal == null) return BadRequest();

        var updated = await _animalService.Update(id, animal);
        if (updated == null) return NotFound();

        return Ok(updated);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(string id, [FromBody] string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return BadRequest("Status is required");

        var updated = await _animalService.ChangeStatus(id, status);
        if (updated == null) return NotFound();

        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}/photos")]
    public async Task<IActionResult> GetPhotos(string id) => Ok(await _animalPhotoService.GetByAnimalId(id));

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/photos")]
    public async Task<IActionResult> UploadPhoto(string id, IFormFile file, [FromForm] bool isPrimary = false)
    {
        var created = await _animalPhotoService.Upload(id, file, isPrimary);
        return Ok(created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/photos/{photoId:long}/primary")]
    public async Task<IActionResult> SetPrimaryPhoto(string id, long photoId)
    {
        var updated = await _animalPhotoService.SetPrimary(photoId);
        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}/photos/{photoId:long}")]
    public async Task<IActionResult> DeletePhoto(string id, long photoId)
    {
        await _animalPhotoService.Delete(photoId);
        return NoContent();
    }
}
