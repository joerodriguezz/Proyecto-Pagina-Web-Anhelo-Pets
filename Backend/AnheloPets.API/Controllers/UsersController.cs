using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserAdminService _userAdminService;

    public UsersController(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _userAdminService.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            return Ok(await _userAdminService.GetById(id));
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateUserStatusDto status)
    {
        return Ok(await _userAdminService.UpdateStatus(id, status));
    }

    [HttpPut("{id}/roles")]
    public async Task<IActionResult> SetRoles(string id, [FromBody] UpdateUserRolesDto roles)
    {
        return Ok(await _userAdminService.SetRoles(id, roles));
    }
}
