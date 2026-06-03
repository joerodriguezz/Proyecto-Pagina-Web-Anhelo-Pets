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
    public IActionResult GetAll() => Ok(_rescateService.GetAll());

    [HttpGet("{id}")]
    public IActionResult GetById(long id)
    {
        var rescate = _rescateService.GetById(id);
        if (rescate == null) return NotFound();
        return Ok(rescate);
    }

    [HttpPost]
    public IActionResult Create([FromBody] RescateDto rescate)
    {
        if (rescate == null) return BadRequest();
        var created = _rescateService.Create(rescate);
        return CreatedAtAction(nameof(GetById), new { id = created.RescateId }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(long id, [FromBody] RescateDto rescate)
    {
        if (rescate == null) return BadRequest();
        var updated = _rescateService.Update(id, rescate);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(long id)
    {
        if (!_rescateService.Delete(id)) return NotFound();
        return NoContent();
    }
}
