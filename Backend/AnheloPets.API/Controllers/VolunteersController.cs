using Microsoft.AspNetCore.Mvc;
using AnheloPets.API.DTOs;
using AnheloPets.API.Services;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VolunteersController : ControllerBase
{
    private readonly IVolunteerService _volunteerService;

    public VolunteersController(IVolunteerService volunteerService)
    {
        _volunteerService = volunteerService;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_volunteerService.GetAll());

    [HttpGet("{id}")]
    public IActionResult GetById(long id)
    {
        var volunteer = _volunteerService.GetById(id);
        if (volunteer == null) return NotFound();
        return Ok(volunteer);
    }

    [HttpPost]
    public IActionResult Create([FromBody] VolunteerDto volunteer)
    {
        if (volunteer == null) return BadRequest();
        var created = _volunteerService.Create(volunteer);
        return CreatedAtAction(nameof(GetById), new { id = created.VolunteerId }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(long id, [FromBody] VolunteerDto volunteer)
    {
        if (volunteer == null) return BadRequest();
        var updated = _volunteerService.Update(id, volunteer);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(long id)
    {
        if (!_volunteerService.Delete(id)) return NotFound();
        return NoContent();
    }
}
