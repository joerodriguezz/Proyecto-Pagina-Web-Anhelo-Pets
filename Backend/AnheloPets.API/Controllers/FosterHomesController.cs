using AnheloPets.API.DTOs;
using AnheloPets.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnheloPets.API.Controllers;

[ApiController]
[Route("api/foster-homes")]
public class FosterHomesController : ControllerBase
{
    private readonly IFosterHomeService _fosterHomeService;
    private readonly IConfiguration _configuration;

    public FosterHomesController(IFosterHomeService fosterHomeService, IConfiguration configuration)
    {
        _fosterHomeService = fosterHomeService;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_fosterHomeService.GetAll());

    [HttpGet("{id:long}")]
    public IActionResult GetById(long id)
    {
        var fosterHome = _fosterHomeService.GetById(id);
        if (fosterHome == null)
            return NotFound(new { message = $"No se encontró la casa cuna con ID {id}." });
        return Ok(fosterHome);
    }

    [HttpPost]
    public IActionResult Create([FromBody] FosterHomeDto fosterHome)
    {
        var created = _fosterHomeService.Create(fosterHome);
        return CreatedAtAction(nameof(GetById), new { id = created.FosterHomeId }, created);
    }

    [HttpPut("{id:long}")]
    public IActionResult Update(long id, [FromBody] FosterHomeDto fosterHome)
    {
        var updated = _fosterHomeService.Update(id, fosterHome);
        if (updated == null)
            return NotFound(new { message = $"No se encontró la casa cuna con ID {id}." });
        return Ok(updated);
    }

    [HttpDelete("{id:long}")]
    public IActionResult Delete(long id)
    {
        if (!_fosterHomeService.Delete(id))
            return NotFound(new { message = $"No se encontró la casa cuna con ID {id}." });
        return NoContent();
    }

    [HttpPost("{id:long}/photo")]
    public async Task<IActionResult> UploadPhoto(long id, IFormFile file)
    {
        // Verificar que la casa cuna existe
        var fosterHome = _fosterHomeService.GetById(id);
        if (fosterHome == null)
            return NotFound(new { message = $"No se encontró la casa cuna con ID {id}." });

        // Validar archivo
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "El archivo es obligatorio." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Solo se permiten imágenes JPG, PNG o WEBP." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "La imagen no puede superar 5MB." });

        // Subir a Supabase Storage
        var supabaseUrl = _configuration["Supabase:Url"];
        var supabaseKey = _configuration["Supabase:ServiceKey"];
        var fileName = $"foster-home-{id}-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var uploadUrl = $"{supabaseUrl}/storage/v1/object/foster-homes/{fileName}";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");
        httpClient.DefaultRequestHeaders.Add("apikey", supabaseKey);

        using var stream = file.OpenReadStream();
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        var response = await httpClient.PutAsync(uploadUrl, content);
        if (!response.IsSuccessStatusCode)
            return StatusCode(500, new { message = "Error al subir la imagen." });

        var publicUrl = $"{supabaseUrl}/storage/v1/object/public/foster-homes/{fileName}";
        return Ok(new { url = publicUrl });
    }
}