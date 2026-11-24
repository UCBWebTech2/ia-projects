using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using APIsEstudioJuridico.Services.Contrato;
using APIsEstudioJuridico.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using SendEmail.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace APIsEstudioJuridico.Controllers
{
    [Authorize()]
    [Route("api/[controller]")]
    [ApiController]
    public class ProcesoController : ControllerBase
    {
        private readonly IProcesoService _procesoService;

        public ProcesoController(IProcesoService procesoService)
        {
            _procesoService = procesoService;
        }

        [HttpGet("Lista")]
        public async Task<ActionResult<List<ProcesoDTO>>> Get()
        {
            var listaProcesos = await _procesoService.Lista();
            return Ok(listaProcesos);
        }
        [HttpGet("ListaHabilitados")]
        public async Task<ActionResult<List<ProcesoDTO>>> GetHabilitados()
        {
            try
            {
                var listaHabilitados = await _procesoService.ListaHabilitados();
                return Ok(listaHabilitados);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ProcesoDTO>> GetPorId(int id)
        {
            var proceso = await _procesoService.ObtenerPorId(id);
            if (proceso == null)
            {
                return NotFound(new { mensaje = "Proceso no encontrado." });
            }
            return Ok(proceso);
        }

        [HttpPost("Crear")]
        public async Task<IActionResult> Crear([FromBody] ProcesoDTO procesoDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(procesoDTO.NombreProceso) || string.IsNullOrEmpty(procesoDTO.DescripcionProceso))
                {
                    return BadRequest(new { mensaje = "El nombre y la descripción del proceso son requeridos." });
                }

                await _procesoService.CrearProcesoAsync(procesoDTO);

                return StatusCode(StatusCodes.Status201Created, new { mensaje = "Proceso creado exitosamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
        [HttpGet("CambiarEstado/{id}")]
        public async Task<IActionResult> CambiarEstadoProceso(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { mensaje = "El ID del proceso no es válido." });
                }

                var resultado = await _procesoService.CambiarEstado(id);
                if (resultado)
                {
                    return StatusCode(StatusCodes.Status200OK, new { mensaje = "El estado del proceso se ha actualizado correctamente." });
                }

                return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "El proceso no se encontró." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }

        // Actualizar un proceso existente
        [HttpPut("Actualizar/{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ProcesoDTO procesoDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Valida los datos antes de continuar
            }
            try
            {
                var actualizado = await _procesoService.ActualizarProceso(id, procesoDTO);
                if (!actualizado)
                {
                    return NotFound(new { mensaje = "Proceso no encontrado o no se pudo actualizar." });
                }

                return Ok(new { mensaje = "Proceso actualizado exitosamente" }); // Proporciona una respuesta clara
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
            
        }

        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _procesoService.EliminarProceso(id);
            if (!eliminado)
            {
                return NotFound(new { mensaje = "Proceso no encontrado o no se pudo eliminar." });
            }

            return NoContent();
        }
    }
}
