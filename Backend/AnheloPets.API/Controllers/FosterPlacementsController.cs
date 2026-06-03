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
    public IActionResult GetAll() => Ok(_fosterPlacementService.GetAll());

    [HttpGet("{id:long}")]
    public IActionResult GetById(long id)
    {
        var placement = _fosterPlacementService.GetById(id);
        if (placement == null) return NotFound();
        return Ok(placement);
    }

    [HttpPost]
    public IActionResult Create([FromBody] FosterPlacementDto placement)
    {
        if (placement == null) return BadRequest();
        var created = _fosterPlacementService.Create(placement);
        return CreatedAtAction(nameof(GetById), new { id = created.AnimalFosterPlacementId }, created);
    }

    [HttpPut("{id:long}")]
    public IActionResult Update(long id, [FromBody] FosterPlacementDto placement)
    {
        if (placement == null) return BadRequest();
        var updated = _fosterPlacementService.Update(id, placement);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:long}")]
    public IActionResult Delete(long id)
    {
        if (!_fosterPlacementService.Delete(id)) return NotFound();
        return NoContent();
    }
}
