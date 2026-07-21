using AnheloPets.API.DTOs;
using AnheloPets.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicalRecordsController : ControllerBase
{
    private readonly IAnimalMedicalRecordService _service;

    public MedicalRecordsController(IAnimalMedicalRecordService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<AnimalMedicalRecordDto>>> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    [HttpGet("animal/{animalId}")]
    public async Task<ActionResult<List<AnimalMedicalRecordDto>>> GetByAnimal(string animalId)
    {
        return Ok(await _service.GetByAnimal(animalId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnimalMedicalRecordDto>> GetById(long id)
    {
        var result = await _service.GetById(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AnimalMedicalRecordDto>> Create(AnimalMedicalRecordDto dto)
    {
        var created = await _service.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.AnimalMedicalRecordId }, created);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        var deleted = await _service.Delete(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
