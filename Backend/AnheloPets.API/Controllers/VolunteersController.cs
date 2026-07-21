using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/volunteers")]
public class VolunteersController : ControllerBase
{
    private readonly IVolunteerService _volunteerService;

    public VolunteersController(IVolunteerService volunteerService)
    {
        _volunteerService = volunteerService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _volunteerService.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            return Ok(await _volunteerService.GetById(id));
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Consulta pública: ¿este correo ya tiene una solicitud? Null si no.</summary>
    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        return Ok(await _volunteerService.GetByEmail(email));
    }

    /// <summary>Envío público del formulario. El usuario debe existir (registrado previamente).</summary>
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitVolunteerApplicationDto application)
    {
        var created = await _volunteerService.Submit(application);
        return CreatedAtAction(nameof(GetById), new { id = created.VolunteerId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateVolunteerDto volunteer)
    {
        return Ok(await _volunteerService.Update(id, volunteer));
    }

    /// <summary>Acciones administrativas: Aprobar | Rechazar | Inactivar | Reactivar.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateVolunteerStatusDto status)
    {
        return Ok(await _volunteerService.UpdateStatus(id, status));
    }
}
