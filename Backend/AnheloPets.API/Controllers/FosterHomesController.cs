using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/foster-homes")]
[Authorize(Roles = "Admin")]
public class FosterHomesController : ControllerBase
{
    private static readonly string[] AllowedPhotoTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxPhotoBytes = 5 * 1024 * 1024;

    private readonly IFosterHomeService _fosterHomeService;
    private readonly ISupabaseStorageService _storageService;

    public FosterHomesController(IFosterHomeService fosterHomeService, ISupabaseStorageService storageService)
    {
        _fosterHomeService = fosterHomeService;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _fosterHomeService.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var fosterHome = await _fosterHomeService.GetById(id);
            return Ok(fosterHome);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FosterHomeDto fosterHome)
    {
        var created = await _fosterHomeService.Create(fosterHome);
        return CreatedAtAction(nameof(GetById), new { id = created.FosterHomeId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] FosterHomeDto fosterHome)
    {
        var updated = await _fosterHomeService.Update(id, fosterHome);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(string id)
    {
        await _fosterHomeService.Deactivate(id);
        return NoContent();
    }

    [HttpPost("{id}/photo")]
    public async Task<IActionResult> UploadPhoto(string id, IFormFile file)
    {
        try
        {
            await _fosterHomeService.GetById(id);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound(new { message = ex.Message });
        }

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "El archivo es obligatorio." });

        if (!AllowedPhotoTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Solo se permiten imágenes JPG, PNG o WEBP." });

        if (file.Length > MaxPhotoBytes)
            return BadRequest(new { message = "La imagen no puede superar 5MB." });

        var path = $"foster-homes/{id}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        string publicUrl;
        using (var stream = file.OpenReadStream())
        {
            publicUrl = await _storageService.UploadPublicAsync("public-media", path, stream, file.ContentType);
        }

        await _fosterHomeService.SetPhotoUrl(id, publicUrl);

        return Ok(new { url = publicUrl });
    }
}
