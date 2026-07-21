using System.Security.Claims;
using AnheloPets.API.DTOs;
using AnheloPets.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        var result = await _userService.Register(request);
        return CreatedAtAction(nameof(Register), new { userId = result.UserId }, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDtoRequest request)
    {
        if (request == null) return BadRequest();

        var result = await _userService.Login(request);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var username = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(username)) return Unauthorized();

        var result = await _userService.GetCurrentUser(username);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpPost("password-reset")]
    public async Task<IActionResult> ResetPasswordByEmail([FromBody] ResetPasswordDto request)
    {
        if (request == null) return BadRequest();
        await _userService.ResetPasswordByEmail(request);
        return NoContent();
    }

    [HttpPut("users/{userId:long}/password")]
    public IActionResult UpdatePassword(long userId, [FromBody] PasswordUpdateDto request)
    {
        if (request == null) return BadRequest();
        if (!_userService.UpdatePassword(userId, request)) return NotFound();

        return NoContent();
    }
}
