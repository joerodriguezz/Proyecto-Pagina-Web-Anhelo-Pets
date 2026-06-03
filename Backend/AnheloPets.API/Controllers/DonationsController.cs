using Microsoft.AspNetCore.Mvc;
using AnheloPets.API.DTOs;
using AnheloPets.API.Services;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonationsController : ControllerBase
{
    private readonly IDonationService _donationService;

    public DonationsController(IDonationService donationService)
    {
        _donationService = donationService;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_donationService.GetAll());

    [HttpGet("{id}")]
    public IActionResult GetById(long id)
    {
        var donation = _donationService.GetById(id);
        if (donation == null) return NotFound();
        return Ok(donation);
    }

    [HttpPost]
    public IActionResult Create([FromBody] DonationDto donation)
    {
        if (donation == null) return BadRequest();
        var created = _donationService.Create(donation);
        return CreatedAtAction(nameof(GetById), new { id = created.DonationId }, created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(long id, [FromBody] DonationDto donation)
    {
        if (donation == null) return BadRequest();
        var updated = _donationService.Update(id, donation);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(long id)
    {
        if (!_donationService.Delete(id)) return NotFound();
        return NoContent();
    }
}
