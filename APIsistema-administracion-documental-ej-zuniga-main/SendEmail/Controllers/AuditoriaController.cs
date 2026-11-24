using APIsEstudioJuridico.DTOs;
using APIsEstudioJuridico.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIsEstudioJuridico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditoriaController : ControllerBase
    {
        private readonly AuditoriaService _auditoriaService;

        public AuditoriaController(AuditoriaService auditoriaService)
        {
            _auditoriaService = auditoriaService;
        }

        [HttpPost("Registrar")]
        public async Task<IActionResult> RegistrarAuditoria([FromBody] RegistrarAuditoriaDTO auditoriaDTO)
        {
            if (auditoriaDTO == null || string.IsNullOrWhiteSpace(auditoriaDTO.Accion) || string.IsNullOrWhiteSpace(auditoriaDTO.Descripcion))
            {
                return BadRequest(new { mensaje = "Parámetros inválidos." });
            }

            try
            {
                await _auditoriaService.RegistrarAuditoriaAsync(auditoriaDTO.IdUsuario, auditoriaDTO.Accion, auditoriaDTO.Descripcion);
                return Ok(new { mensaje = "Registro de auditoría exitoso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor.", detalle = ex.Message });
            }
        }
        [Authorize]
        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> ListaAuditorias()
        {
            List<AuditoriumDTO> _lista;

            try
            {
                _lista = await _auditoriaService.ListaAuditoriasAsync();

                if (_lista == null || !_lista.Any())
                {
                    return Ok(new { value = new List<AuditoriumDTO>(), mensaje = "No se encontraron auditorías." });
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", response = _lista });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }

    }    
    public class RegistrarAuditoriaDTO
    {
        public int IdUsuario { get; set; }
        public string? Accion { get; set; }
        public string? Descripcion { get; set; }
    }
}
