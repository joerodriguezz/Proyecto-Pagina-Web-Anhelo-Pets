using AnheloPets.API.DTOs;
using AnheloPets.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/foster-homes")]
public class FosterHomesController : ControllerBase
{
    private readonly IFosterHomeService _fosterHomeService;

    public FosterHomesController(IFosterHomeService fosterHomeService)
    {
        _fosterHomeService = fosterHomeService;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_fosterHomeService.GetAll());

    [HttpGet("{id:long}")]
    public IActionResult GetById(long id)
    {
        var fosterHome = _fosterHomeService.GetById(id);
        if (fosterHome == null) return NotFound();
        return Ok(fosterHome);
    }

    [HttpPost]
    public IActionResult Create([FromBody] FosterHomeDto fosterHome)
    {
        if (fosterHome == null) return BadRequest();
        var created = _fosterHomeService.Create(fosterHome);
        return CreatedAtAction(nameof(GetById), new { id = created.FosterHomeId }, created);
    }

    [HttpPut("{id:long}")]
    public IActionResult Update(long id, [FromBody] FosterHomeDto fosterHome)
    {
        if (fosterHome == null) return BadRequest();
        var updated = _fosterHomeService.Update(id, fosterHome);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:long}")]
    public IActionResult Delete(long id)
    {
        if (!_fosterHomeService.Delete(id)) return NotFound();
        return NoContent();
    }
}
