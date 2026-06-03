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
    public IActionResult Register([FromBody] RegisterUserDto request)
    {
        if (request == null) return BadRequest();

        var user = _userService.Register(request);
        return CreatedAtAction(nameof(Register), new { id = user.UserId }, user);
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto request)
    {
        if (request == null) return BadRequest();

        var user = _userService.Login(request);
        if (user == null) return Unauthorized();

        return Ok(user);
    }

    [HttpPut("users/{userId:long}/password")]
    public IActionResult UpdatePassword(long userId, [FromBody] PasswordUpdateDto request)
    {
        if (request == null) return BadRequest();
        if (!_userService.UpdatePassword(userId, request)) return NotFound();

        return NoContent();
    }
}
