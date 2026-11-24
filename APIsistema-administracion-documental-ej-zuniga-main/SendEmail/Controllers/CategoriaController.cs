using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendEmail.Context;
using SendEmail.Models;
using SendEmail.DTOs;
using APIsEstudioJuridico.Services;
using Microsoft.AspNetCore.Authorization;

namespace SendEmail.Controllers
{
    [Authorize()]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly CategoriaService _categoriaService;
        public CategoriaController(CategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }
        [HttpGet]
        [Route("Lista")]
        public async Task<ActionResult<List<CategoriaDTO>>>Get()
        {
            return Ok(await _categoriaService.Lista());
        }
        [HttpGet("ListaHabilitadas")]
        public async Task<ActionResult<List<ProcesoDTO>>> GetHabilitados()
        {
            try
            {
                var listaHabilitadas = await _categoriaService.ListaHabilitadas();
                return Ok(listaHabilitadas);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaDTO>> GetPorId(int id)
        {
            var categoria = await _categoriaService.ObtenerPorId(id);
            if (categoria == null)
            {
                return NotFound();
            }

            return Ok(categoria);
        }
        [HttpPost("Crear")]
        public async Task<IActionResult> Crear([FromBody] CategoriaDTO categoriaDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(categoriaDTO.NombreCategoria) || string.IsNullOrEmpty(categoriaDTO.DescripcionCategoria))
                {
                    return BadRequest(new { mensaje = "El nombre y la descripción son requeridos." });
                }

                await _categoriaService.CrearCategoriaAsync(categoriaDTO);

                return StatusCode(StatusCodes.Status201Created, new { mensaje = "Categoría creada exitosamente." });
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
        public async Task<IActionResult> CambiarEstadoCategoria(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { mensaje = "El ID de la categoría no es válido." });
                }

                var resultado = await _categoriaService.CambiarEstado(id);
                if (resultado)
                {
                    return StatusCode(StatusCodes.Status200OK, new { mensaje = "El estado de la categoría se ha actualizado correctamente." });
                }

                return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "La categoría no se encontró." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }


        [HttpPut("Actualizar/{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] CategoriaDTO categoriaDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Valida los datos antes de continuar
            }

            try
            {
                var actualizado = await _categoriaService.Actualizar(id, categoriaDTO);
                if (!actualizado)
                {
                    return NotFound(new { mensaje = "Categoría no encontrada" });
                }

                return Ok(new { mensaje = "Categoría actualizada exitosamente" }); // Proporciona una respuesta clara
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpPut("Actualizar1/{id}")]
        public async Task<ActionResult> Actualizar1(int id, [FromBody] CategoriaDTO categoriaDTO)
        {
            var actualizado = await _categoriaService.Actualizar(id, categoriaDTO);
            if (!actualizado)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}
