using System.Security.Claims;
using AnheloPets.API.DTOs;
using AnheloPets.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/adoption-requests")]
public class AdoptionRequestsController : ControllerBase
{
    private readonly IAdoptionRequestService _service;

    public AdoptionRequestsController(IAdoptionRequestService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var request = await _service.GetByIdAsync(id);
        if (request == null) return NotFound();
        return Ok(request);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SubmitAdoptionRequestDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var created = await _service.CreateAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.AdoptionRequestId }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateAdoptionRequestStatusDto dto)
    {
        var updated = await _service.UpdateStatusAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }
}
