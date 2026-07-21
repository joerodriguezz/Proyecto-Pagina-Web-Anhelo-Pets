using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/veterinarians")]
public class VeterinariansController : ControllerBase
{
    private readonly IVeterinarianService _veterinarianService;

    public VeterinariansController(IVeterinarianService veterinarianService)
    {
        _veterinarianService = veterinarianService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _veterinarianService.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            return Ok(await _veterinarianService.GetById(id));
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVeterinarianDto veterinarian)
    {
        var created = await _veterinarianService.Create(veterinarian);
        return CreatedAtAction(nameof(GetById), new { id = created.VeterinarianId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateVeterinarianDto veterinarian)
    {
        var updated = await _veterinarianService.Update(id, veterinarian);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(string id)
    {
        await _veterinarianService.Deactivate(id);
        return NoContent();
    }
}
