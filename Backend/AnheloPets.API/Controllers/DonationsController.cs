using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AnheloPets.API.DTOs;
using AnheloPets.API.Services;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DonationsController : ControllerBase
{
    private readonly IDonationService _donationService;

    public DonationsController(IDonationService donationService)
    {
        _donationService = donationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _donationService.GetAllAsync());

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var donation = await _donationService.GetByIdAsync(id);
        if (donation == null) return NotFound();
        return Ok(donation);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SubmitDonationDto donation)
    {
        var created = await _donationService.CreateAsync(donation);
        return CreatedAtAction(nameof(GetById), new { id = created.DonationId }, created);
    }

    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateDonationStatusDto status)
    {
        var updated = await _donationService.UpdateStatusAsync(id, status);
        return Ok(updated);
    }
}
