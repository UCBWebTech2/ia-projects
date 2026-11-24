using APIsEstudioJuridico.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SendEmail.DTOs;

namespace SendEmail.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ModelRegistryController : ControllerBase
{
    private readonly ModelRegistryService _service;

    public ModelRegistryController(ModelRegistryService service)
    {
        _service = service;
    }

    [HttpGet("Listar")]
    public async Task<IActionResult> Listar() => Ok(await _service.ListarAsync());

    [HttpGet("Activo")]
    public async Task<IActionResult> Activo() => Ok(await _service.ObtenerActivoAsync());

    [HttpPost("Registrar")]
    public async Task<IActionResult> Registrar([FromBody] ModelRegistryDTO dto, [FromQuery] bool activar = false)
    {
        if (string.IsNullOrWhiteSpace(dto.Version))
            return BadRequest(new { mensaje = "Version es obligatoria." });
        var created = await _service.RegistrarAsync(dto, activar);
        return Ok(created);
    }

    [HttpPost("Activar/{id:int}")]
    public async Task<IActionResult> Activar(int id)
    {
        var ok = await _service.ActivarAsync(id);
        if (!ok) return NotFound(new { mensaje = "Registro no encontrado." });
        return Ok(new { mensaje = "Modelo activado." });
    }
}
