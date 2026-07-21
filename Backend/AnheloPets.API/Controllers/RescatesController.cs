using Microsoft.AspNetCore.Mvc;
using AnheloPets.API.DTOs;
using AnheloPets.API.Services;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RescatesController : ControllerBase
{
    private readonly IRescateService _rescateService;

    public RescatesController(IRescateService rescateService)
    {
        _rescateService = rescateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _rescateService.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var rescate = await _rescateService.GetById(id);
        if (rescate == null)
            return NotFound(new { message = $"No se encontró el rescate con ID {id}." });
        return Ok(rescate);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RescateDto rescate)
    {
        var created = await _rescateService.Create(rescate);
        return CreatedAtAction(nameof(GetById), new { id = created.RescateId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] RescateDto rescate)
    {
        var updated = await _rescateService.Update(id, rescate);
        if (updated == null)
            return NotFound(new { message = $"No se encontró el rescate con ID {id}." });
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Close(long id)
    {
        if (!await _rescateService.Close(id))
            return NotFound(new { message = $"No se encontró el rescate con ID {id}." });
        return NoContent();
    }
}
