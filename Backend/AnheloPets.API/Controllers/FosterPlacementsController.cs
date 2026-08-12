using AnheloPets.API.DTOs;
using AnheloPets.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/foster-placements")]
public class FosterPlacementsController : ControllerBase
{
    private readonly IFosterPlacementService _fosterPlacementService;

    public FosterPlacementsController(IFosterPlacementService fosterPlacementService)
    {
        _fosterPlacementService = fosterPlacementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _fosterPlacementService.GetAll());

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var placement = await _fosterPlacementService.GetById(id);
        if (placement == null) return NotFound();
        return Ok(placement);
    }

    /// <summary>Asignación vigente de un animal (null si no tiene casa cuna asignada).</summary>
    [HttpGet("by-animal/{animalId}")]
    public async Task<IActionResult> GetActiveByAnimal(string animalId)
    {
        return Ok(await _fosterPlacementService.GetActiveByAnimalId(animalId));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FosterPlacementDto placement)
    {
        if (placement == null) return BadRequest();
        var created = await _fosterPlacementService.Create(placement);
        return CreatedAtAction(nameof(GetById), new { id = created.AnimalFosterPlacementId }, created);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] FosterPlacementDto placement)
    {
        if (placement == null) return BadRequest();
        var updated = await _fosterPlacementService.Update(id, placement);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        if (!await _fosterPlacementService.Delete(id)) return NotFound();
        return NoContent();
    }
}
