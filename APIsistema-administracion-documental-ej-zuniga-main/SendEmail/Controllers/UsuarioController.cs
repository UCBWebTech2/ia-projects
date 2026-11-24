using APIsEstudioJuridico.DTOs;
using APIsEstudioJuridico.Services;
using APIsEstudioJuridico.Services.Contrato;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendEmail.Context;
using SendEmail.Custom;
using SendEmail.DTOs;
using SendEmail.Models;
using SendEmail.Services;

namespace APIsEstudioJuridico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _bdpruebaProyectoContext;
        private readonly Utilidades _utilidades;
        private readonly IUsuarioService _usuarioServicio;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly string _interfazUrl;
        
        public UsuarioController(AppDbContext bdpruebaProyectoContext, Utilidades utilidades, IUsuarioService usuarioService, IEmailService emailService, IConfiguration configuration, IPasswordHasher<Usuario> passwordHasher)
        {
            _emailService = emailService;
            _bdpruebaProyectoContext = bdpruebaProyectoContext;
            _utilidades = utilidades;
            _usuarioServicio = usuarioService;
            _passwordHasher = passwordHasher;
            _interfazUrl = configuration["Interfaz:Url"] ?? throw new ArgumentNullException("Interfaz:Url configuration is missing");
        }
        [HttpGet]
        [Route("Lista")]
        public async Task<ActionResult<List<UsuarioDTO2>>> Lista()
        {
            var usuarios = await _usuarioServicio.ObtenerUsuariosConRolAbogado();
            return Ok(usuarios);
        }
        [HttpGet]
        [Route("ObtenerUsuario")]
        public async Task<IActionResult> ObtenerUsuario(string token)
        {
            try
            {
                var usuario = await _usuarioServicio.ConfirmarPorToken(token);
                bool usuarioExiste = usuario;
                return Ok(usuarioExiste);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocurrió un error: {ex.Message}");
            }
        }
        [HttpGet]
        [Route("ObtenerUsuarioConfirmado")]
        public async Task<IActionResult> ObtenerUsuarioConfirmado(string token)
        {
            try
            {
                var usuario = await _usuarioServicio.ConfirmarPorTokenConfirmado(token);
                bool usuarioExiste = usuario;
                return Ok(usuarioExiste);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocurrió un error: {ex.Message}");
            }
        }
        [HttpGet]
        [Route("ObtenerCorreoPorToken")]
        public async Task<IActionResult> ObtenerCorreoPorToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { mensaje = "El token es obligatorio" });
                }
                var correo = await _usuarioServicio.ObtenerCorreoPorToken(token);
                if (!string.IsNullOrEmpty(correo))
                {
                    return Ok(correo); // Devuelve el correo si se encuentra
                }

                return NotFound(new { Mensaje = "No se encontró un usuario asociado al token proporcionado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensaje = $"Ocurrió un error: {ex.Message}" });
            }
        }
        [HttpGet]
        [Route("ObtenerUsuarioPorCorreo")]
        public async Task<IActionResult> ObtenerUsuarioPorCorreo(string correo)
        {
            try
            {
                var usuario = await _usuarioServicio.ObtenerUsuario(correo);
                if (usuario != null)
                {
                    return Ok(usuario);
                }
                else
                {
                    return NotFound(new { Mensaje = "Usuario no encontrado." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocurrió un error: {ex.Message}");
            }
        }
        [HttpPost]
        [Route("RegistroUsuario")]
        public async Task<IActionResult> RegistroUsuario(RegistroUsuarioDTO rUsuario)
        {
            try
            {
                UsuarioDTO objeto = new UsuarioDTO();
                objeto.NombreUsuario=rUsuario.NombreUsuario;
                objeto.CorreoElectronico = rUsuario.CorreoElectronico;
                objeto.Rol = rUsuario.Rol;
                objeto.Contrasenia = Utilidades.GenerateRandomPassword();
                objeto.Contrasenia = _passwordHasher.HashPassword(
                    new Usuario
                    {
                        NombreUsuario = objeto.NombreUsuario,
                        CorreoElectronico = objeto.CorreoElectronico,
                        Rol = objeto.Rol
                    },
                    objeto.Contrasenia);
                objeto.Token = Utilidades.GenerarToken();
                Usuario usuario_creado = await _usuarioServicio.SaveUsuario2(objeto);
                if (usuario_creado.IdUsuario != 0)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "Plantilla", "Confirmar.html");
                    string content = await System.IO.File.ReadAllTextAsync(path);
                    string url = $"{_interfazUrl}/Acceso/Confirmar2?token={objeto.Token}";

                    string htmlBody = string.Format(content, objeto.NombreUsuario, url);

                    EmailDTO emailDTO = new EmailDTO()
                    {
                        Para = objeto.CorreoElectronico,
                        Asunto = "Confirmación de Correo",
                        Contenido = htmlBody
                    };

                    bool enviado = _emailService.Enviar(emailDTO); // Llama al método Enviar

                    if (enviado)
                    {
                        return StatusCode(StatusCodes.Status200OK,
                            new { isSuccess = true, message = $"Su cuenta ha sido creada. Hemos enviado un mensaje al correo {objeto.CorreoElectronico} para confirmar su cuenta." });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            new { isSuccess = false, message = "No se pudo enviar el correo de confirmación." });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { isSuccess = false, message = "No se pudo crear su cuenta." });
                }


            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocurrió un error: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("RegistrarUsuario")]
        public async Task<IActionResult> RegistrarUsuario(RegistroUsuarioDTO rUsuario)
        {
            try
            {
                UsuarioDTO objeto = new UsuarioDTO
                {
                    NombreUsuario = rUsuario.NombreUsuario,
                    CorreoElectronico = rUsuario.CorreoElectronico,
                    Rol = rUsuario.Rol,
                    Contrasenia = Utilidades.GenerateRandomPassword(),
                    Confirmado = true,
                    Restablecer = true,
                    Token = Utilidades.GenerarToken()
                };

                objeto.Contrasenia = _passwordHasher.HashPassword(
                    new Usuario
                    {
                        NombreUsuario = objeto.NombreUsuario,
                        CorreoElectronico = objeto.CorreoElectronico,
                        Rol = objeto.Rol
                    },
                    objeto.Contrasenia);

                Usuario usuario_creado = await _usuarioServicio.SaveUsuario2(objeto);

                if (usuario_creado.IdUsuario != 0)
                {
                    // Enviar correo de restablecimiento
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "Plantilla", "Restablecer.html");
                    string content = await System.IO.File.ReadAllTextAsync(path);
                    string url = $"{_interfazUrl}/Acceso/Actualizar?token={objeto.Token}";

                    // Personalizar la plantilla del correo con los valores del usuario
                    string htmlBody = string.Format(content, objeto.NombreUsuario, url);

                    EmailDTO emailDTO = new EmailDTO
                    {
                        Para = objeto.CorreoElectronico,
                        Asunto = "Restablecimiento de Contraseña",
                        Contenido = htmlBody
                    };

                    // Enviar correo de forma sincrónica
                    bool enviado = _emailService.Enviar(emailDTO);

                    if (enviado)
                    {
                        return StatusCode(StatusCodes.Status200OK,
                            new { isSuccess = true, message = $"El usuario ha sido creado y se envió un correo a {objeto.CorreoElectronico} para restablecer la contraseña." });
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            new { isSuccess = false, message = "No se pudo enviar el correo de restablecimiento." });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { isSuccess = false, message = "No se pudo crear el usuario." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { isSuccess = false, message = $"Ocurrió un error: {ex.Message}" });
            }
        }
        [HttpPut]
        [Route("ActualizarUsuario")]
        public async Task<IActionResult> ActualizarUsuario(ActualizarUsuarioDTO actualizarUsuario)
        {
            try
            {
                var usuarioActual = await _usuarioServicio.ObtenerUsuarioPorId(actualizarUsuario.IdUsuario);
                if (usuarioActual == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { isSuccess = false, message = "Usuario no encontrado." });
                }

                bool correoCambiado = usuarioActual.CorreoElectronico != actualizarUsuario.CorreoElectronico;

                usuarioActual.NombreUsuario = actualizarUsuario.NombreUsuario;
                usuarioActual.CorreoElectronico = actualizarUsuario.CorreoElectronico;
                usuarioActual.Rol = actualizarUsuario.Rol;
                usuarioActual.Confirmado = actualizarUsuario.Confirmar;
                usuarioActual.FechaModificacion = DateTime.Now;

                if (correoCambiado)
                {
                    usuarioActual.Token = Utilidades.GenerarToken();
                    usuarioActual.Restablecer = true;

                    string path = Path.Combine(Directory.GetCurrentDirectory(), "Plantilla", "Restablecer.html");
                    string content = await System.IO.File.ReadAllTextAsync(path);
                    string url = $"{_interfazUrl}/Acceso/Actualizar?token={usuarioActual.Token}";

                    string htmlBody = string.Format(content, usuarioActual.NombreUsuario, url);

                    EmailDTO emailDTO = new EmailDTO
                    {
                        Para = usuarioActual.CorreoElectronico,
                        Asunto = "Restablecimiento de Contraseña",
                        Contenido = htmlBody
                    };

                    bool enviado = _emailService.Enviar(emailDTO);
                    if (!enviado)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError,
                            new { isSuccess = false, message = "No se pudo enviar el correo de restablecimiento." });
                    }
                }
                bool actualizado = await _usuarioServicio.ActualizarUsuario(usuarioActual);

                if (actualizado)
                {
                    return StatusCode(StatusCodes.Status200OK,
                        new { isSuccess = true, message = "El usuario ha sido actualizado correctamente." });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { isSuccess = false, message = "No se pudo actualizar el usuario." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { isSuccess = false, message = $"Ocurrió un error: {ex.Message}" });
            }
        }

    }
}
