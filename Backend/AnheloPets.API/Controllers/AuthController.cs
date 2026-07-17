using AnheloPets.API.DTOs;
using AnheloPets.API.Services;
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
        if (result.Message == "Datos incorrectos") return Unauthorized(result);

        return Ok(result);
    }

    [HttpPut("users/{userId:long}/password")]
    public IActionResult UpdatePassword(long userId, [FromBody] PasswordUpdateDto request)
    {
        if (request == null) return BadRequest();
        if (!_userService.UpdatePassword(userId, request)) return NotFound();

        return NoContent();
    }
}
