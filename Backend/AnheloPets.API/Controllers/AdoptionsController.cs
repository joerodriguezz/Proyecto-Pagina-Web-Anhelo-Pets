using Microsoft.AspNetCore.Mvc;
using AnheloPets.API.DTOs;
using AnheloPets.API.Services;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdoptionsController : ControllerBase
{
    private readonly IAdoptionService _adoptionService;

    public AdoptionsController(IAdoptionService adoptionService)
    {
        _adoptionService = adoptionService;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_adoptionService.GetAll());

    [HttpGet("{id}")]
    public IActionResult GetById(long id)
    {
        var adoption = _adoptionService.GetById(id);
        if (adoption == null) return NotFound();
        return Ok(adoption);
    }

    [HttpPost]
    public IActionResult Create([FromBody] AdoptionDto adoption)
    {
        if (adoption == null) return BadRequest();
        var created = _adoptionService.Create(adoption);
        return CreatedAtAction(nameof(GetById), new { id = created.AdoptionId }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(long id, [FromBody] AdoptionDto adoption)
    {
        if (adoption == null) return BadRequest();
        var updated = _adoptionService.Update(id, adoption);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(long id)
    {
        if (!_adoptionService.Delete(id)) return NotFound();
        return NoContent();
    }
}
