using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendEmail.Custom;
using SendEmail.Models;
using APIsEstudioJuridico.Services.Contrato;
using Microsoft.AspNetCore.Authorization;
using SendEmail.Context;
using SendEmail.DTOs;
using SendEmail.ViewModels;
using APIsEstudioJuridico.DTOs;
using SendEmail.Services;
using Microsoft.AspNetCore.Identity;


namespace Acceso.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class AccesoController : ControllerBase
    {
        private readonly AppDbContext _bdpruebaProyectoContext;
        private readonly Utilidades _utilidades;
        private readonly IUsuarioService _usuarioServicio;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly ILogger<AccesoController> _logger;
        private readonly string _interfazUrl;
        public AccesoController(
            AppDbContext bdpruebaProyectoContext,
            Utilidades utilidades,
            IUsuarioService usuarioService,
            IEmailService emailService,
            IConfiguration configuration,
            IPasswordHasher<Usuario> passwordHasher,
            ILogger<AccesoController> logger)
        {
            _bdpruebaProyectoContext = bdpruebaProyectoContext;
            _utilidades = utilidades;
            _usuarioServicio = usuarioService;
            _emailService = emailService;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _interfazUrl = configuration["Interfaz:Url"] ?? throw new ArgumentNullException("Interfaz:Url configuration is missing");
        }

        private static bool EsHashSha256(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor) || valor.Length != 64)
            {
                return false;
            }

            return valor.All(c =>
                (c >= '0' && c <= '9') ||
                (c >= 'a' && c <= 'f') ||
                (c >= 'A' && c <= 'F'));
        }

        private bool VerificarContrasenia(Usuario usuario, string contrasenia)
        {
            if (string.IsNullOrWhiteSpace(usuario.Contrasenia))
            {
                return false;
            }

            if (EsHashSha256(usuario.Contrasenia))
            {
                string contraseniaSha256 = _utilidades.encriptarSHA256(contrasenia);
                return usuario.Contrasenia.Equals(contraseniaSha256, StringComparison.OrdinalIgnoreCase);
            }

            PasswordVerificationResult resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.Contrasenia,
                contrasenia);

            return resultado != PasswordVerificationResult.Failed;
        }
        [HttpPost]
        [Route("Registrarse")]
        public async Task<IActionResult> Registrarse(UsuarioDTO objeto)
        {
            if (objeto.Contrasenia != objeto.ConfirmarClave)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { isSuccess = false, message = "Las contraseñas no coinciden" });
            }

            var usuarioExistente = await _bdpruebaProyectoContext.Usuarios
                                        .FirstOrDefaultAsync(u => u.CorreoElectronico == objeto.CorreoElectronico);

            if (usuarioExistente != null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { isSuccess = false, message = "El correo ya se encuentra registrado" });
            }

            var modeloUsuario = new Usuario
            {
                NombreUsuario = objeto.NombreUsuario,
                CorreoElectronico = objeto.CorreoElectronico,
                Rol = "Abogado",
                FechaCreacion = DateTime.Now,
                FechaModificacion = DateTime.Now,
                Restablecer = false,
                Confirmado = false,
                Token = Utilidades.GenerarToken()
            };
            modeloUsuario.Contrasenia = _passwordHasher.HashPassword(modeloUsuario, objeto.Contrasenia);

            await _bdpruebaProyectoContext.AddAsync(modeloUsuario);
            await _bdpruebaProyectoContext.SaveChangesAsync();

            if (modeloUsuario.IdUsuario != 0)
            {
                // Enviar correo de confirmación
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Plantilla", "Confirmar.html");
                string content = await System.IO.File.ReadAllTextAsync(path);
                //string url = $"{_interfazUrl}/Acceso/Confirmar?idUsuario={modeloUsuario.IdUsuario}";
                string url = $"{_interfazUrl}/Acceso/Confirmar?token={modeloUsuario.Token}";

                string htmlBody = string.Format(content, modeloUsuario.NombreUsuario, url);

                EmailDTO emailDTO = new EmailDTO()
                {
                    Para = modeloUsuario.CorreoElectronico,
                    Asunto = "Correo confirmación",
                    Contenido = htmlBody
                };

                bool enviado = _emailService.Enviar(emailDTO);

                if (enviado)
                {
                    return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = $"Su cuenta ha sido creada. Hemos enviado un mensaje al correo {modeloUsuario.CorreoElectronico} para confirmar su cuenta" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = "No se pudo enviar el correo de confirmación" });
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = "No se pudo crear su cuenta" });
            }
        }

        [HttpPost]
        [Route("Restablecer")]
        public async Task<IActionResult> Restablecer(string correo)
        {
            var usuario = await _usuarioServicio.Obtener(correo);
            if (usuario != null)
            {
                if (string.IsNullOrWhiteSpace(usuario.Contrasenia) || string.IsNullOrWhiteSpace(usuario.Token))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        isSuccess = false,
                        message = "El usuario no tiene contraseña o token válido."
                    });
                }

                string contraseniaActual = usuario.Contrasenia;
                string tokenRestablecimiento = usuario.Token;
                bool respuesta = await _usuarioServicio.Restablecer(1, contraseniaActual, tokenRestablecimiento);

                if (respuesta)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "Plantilla", "Restablecer.html");
                    string content = await System.IO.File.ReadAllTextAsync(path);
                    //string url = $"{_interfazUrl}/Acceso/Actualizar?idUsuario={usuario.IdUsuario}";
                    string url = $"{_interfazUrl}/Acceso/Actualizar?token={tokenRestablecimiento}";

                    string htmlBody = string.Format(content, usuario.NombreUsuario, url);

                    EmailDTO emailDTO = new EmailDTO()
                    {
                        Para = correo,
                        Asunto = "Restablecer cuenta",
                        Contenido = htmlBody
                    };

                    bool enviado = _emailService.Enviar(emailDTO);
                    if (enviado)
                    {
                        return Ok(new { Restablecido = true, Mensaje = "Se ha enviado un correo para restablecer la cuenta." });
                    }
                    else
                    {
                        return StatusCode(500, new { Restablecido = false, Mensaje = "No se pudo enviar el correo." });
                    }
                }
                else
                {
                    return StatusCode(500, new { Restablecido = false, Mensaje = "No se pudo restablecer la cuenta." });
                }
            }
            else
            {
                return NotFound(new { Restablecido = false, Mensaje = "No se encontraron coincidencias con el correo." });
            }
        }

        [HttpGet]
        [Route("Actualizar")]
        public IActionResult Actualizar([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token inválido o faltante" });
            }

            // Si el token es válido, devolver alguna información o simplemente confirmar que es válido
            return Ok(new { token });
        }

        [HttpPost]
        [Route("Actualizar")]
        public async Task<IActionResult> Actualizar([FromBody] ActualizarContraseniaDTO objeto)
        {
            if (objeto == null)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Datos no proporcionados."
                });
            }

            if (string.IsNullOrWhiteSpace(objeto.NuevaContrasenia) ||
                string.IsNullOrWhiteSpace(objeto.ConfirmarClave))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Debe ingresar y confirmar la nueva contraseña."
                });
            }
            // Verificar que las nuevas contraseñas coincidan
            if (objeto.NuevaContrasenia != objeto.ConfirmarClave)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { isSuccess = false, message = "Las contraseñas no coinciden" });
            }

            // Verificar que el token no esté vacío o nulo
            if (string.IsNullOrEmpty(objeto.Token))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { isSuccess = false, message = "Token inválido o faltante" });
            }

            // Buscar al usuario por el token
            var usuario = await _usuarioServicio.ObtenerUsuarioPorToken(objeto.Token);

            if (usuario == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { isSuccess = false, message = "Token inválido o expirado" });
            }

            // Actualizar la contraseña del usuario
            string nuevaContraseniaHash = _passwordHasher.HashPassword(usuario, objeto.NuevaContrasenia);

            bool respuesta = await _usuarioServicio.Restablecer(
                0,
                nuevaContraseniaHash,
                objeto.Token
            );

            if (respuesta)
            {
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Contraseña actualizada correctamente" });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = "No se pudo actualizar la contraseña" });
            }
        }
        [HttpPost]
        [Route("RegistroUsuario")]
        public async Task<IActionResult> RegistroUsuario(UsuarioDTO objeto)
        {
            var usuarioParaHash = new Usuario
            {
                NombreUsuario = objeto.NombreUsuario,
                CorreoElectronico = objeto.CorreoElectronico,
                Rol = objeto.Rol
            };
            objeto.Contrasenia = _passwordHasher.HashPassword(usuarioParaHash, objeto.Contrasenia);

            Usuario usuario_creado = await _usuarioServicio.SaveUsuario(objeto);

            if (usuario_creado.IdUsuario != 0)
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = true });
            else
                return StatusCode(StatusCodes.Status200OK, new { isSuccess = false });
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO objeto)
        {
            try
            {
                if (objeto == null ||
                    string.IsNullOrWhiteSpace(objeto.CorreoElectronico) ||
                    string.IsNullOrWhiteSpace(objeto.Contrasenia))
                {
                    return BadRequest(new
                    {
                        isSuccess = false,
                        message = "El correo y la contraseña son obligatorios.",
                        token = "",
                        usuario = (object?)null
                    });
                }

                var correo = objeto.CorreoElectronico.Trim();
                var contraseniaIngresada = objeto.Contrasenia;

                var usuarioEncontrado = await _bdpruebaProyectoContext.Usuarios
                    .FirstOrDefaultAsync(u => u.CorreoElectronico == correo);

                if (usuarioEncontrado == null || !VerificarContrasenia(usuarioEncontrado, contraseniaIngresada))
                {
                    return Unauthorized(new
                    {
                        isSuccess = false,
                        message = "Correo o contraseña incorrectos.",
                        token = "",
                        usuario = (object?)null
                    });
                }

                if (!usuarioEncontrado.Confirmado)
                {
                    return Ok(new
                    {
                        isSuccess = false,
                        message = $"Falta confirmar su cuenta. Se le envió un correo a {usuarioEncontrado.CorreoElectronico}.",
                        token = "",
                        usuario = (object?)null
                    });
                }

                if (usuarioEncontrado.Restablecer)
                {
                    return Ok(new
                    {
                        isSuccess = false,
                        message = $"Se ha solicitado restablecer su cuenta. Revise la bandeja del correo {usuarioEncontrado.CorreoElectronico}.",
                        token = "",
                        usuario = (object?)null
                    });
                }

                if (EsHashSha256(usuarioEncontrado.Contrasenia))
                {
                    usuarioEncontrado.Contrasenia = _passwordHasher.HashPassword(usuarioEncontrado, contraseniaIngresada);
                    usuarioEncontrado.FechaModificacion = DateTime.Now;
                    _bdpruebaProyectoContext.Usuarios.Update(usuarioEncontrado);
                    await _bdpruebaProyectoContext.SaveChangesAsync();
                }

                var usuarioVM = new UsuarioVM
                {
                    IdUsuario = usuarioEncontrado.IdUsuario,
                    NombreUsuario = usuarioEncontrado.NombreUsuario,
                    CorreoElectronico = usuarioEncontrado.CorreoElectronico,
                    Rol = usuarioEncontrado.Rol,
                    Token = usuarioEncontrado.Token
                };

                return Ok(new
                {
                    isSuccess = true,
                    message = "Inicio de sesión correcto.",
                    token = _utilidades.generarJWT(usuarioEncontrado),
                    usuario = usuarioVM
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al iniciar sesión para el correo proporcionado.");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    isSuccess = false,
                    message = "Ocurrió un error interno al iniciar sesión.",
                    token = "",
                    usuario = (object?)null
                });
            }
        }
        [HttpPost]
        [Route("ActualizarContraseniaDesdeApi")]
        public async Task<IActionResult> ActualizarContraseniaDesdeApi([FromBody] ActualizarContraseniaDTO actualizarContraseniaDTO)
        {
            if (actualizarContraseniaDTO == null)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Datos no proporcionados"
                });
            }

            if (string.IsNullOrWhiteSpace(actualizarContraseniaDTO.NuevaContrasenia) ||
                string.IsNullOrWhiteSpace(actualizarContraseniaDTO.ConfirmarClave) ||
                string.IsNullOrWhiteSpace(actualizarContraseniaDTO.Token))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Debe enviar token, nueva contraseña y confirmación."
                });
            }
            if (actualizarContraseniaDTO.NuevaContrasenia != actualizarContraseniaDTO.ConfirmarClave)
            {
                return BadRequest(new { isSuccess = false, message = "Las contraseñas no coinciden" });
            }
            string tokenRestablecimiento = actualizarContraseniaDTO.Token;
            string nuevaContrasenia = actualizarContraseniaDTO.NuevaContrasenia;
            var usuario = await _bdpruebaProyectoContext.Usuarios
                .FirstOrDefaultAsync(u => u.Token == tokenRestablecimiento);

            if (usuario == null)
            {
                return NotFound(new { isSuccess = false, message = "Usuario no encontrado" });
            }

            string nuevaContraseniaHash = _passwordHasher.HashPassword(usuario, nuevaContrasenia);

            usuario.Contrasenia = nuevaContraseniaHash;

            bool respuesta = await _usuarioServicio.Actualizar(
                0,
                nuevaContraseniaHash,
                tokenRestablecimiento
            );

            if (respuesta)
            {
                return Ok(new { isSuccess = true, message = "Contraseña actualizada correctamente" });
            }
            else
            {
                return BadRequest(new { isSuccess = false, message = "Usuario no actualizado." });
            }
            
        }

        [HttpPost]
        [Route("ActualizarContrasenia")]
        public async Task<IActionResult> ActualizarContrasenia([FromBody] ActualizarContraseniaDTO objeto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (objeto == null)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Datos no proporcionados"
                });
            }

            if (string.IsNullOrWhiteSpace(objeto.Token) ||
                string.IsNullOrWhiteSpace(objeto.ContraseniaActual) ||
                string.IsNullOrWhiteSpace(objeto.NuevaContrasenia))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Debe enviar token, contraseña actual y nueva contraseña."
                });
            }
            string token = objeto.Token;
            string contraseniaActual = objeto.ContraseniaActual;
            string nuevaContrasenia = objeto.NuevaContrasenia;

            var usuario = await _bdpruebaProyectoContext.Usuarios
                .FirstOrDefaultAsync(u => u.Token == token);

            if (usuario == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { isSuccess = false, message = "Usuario no encontrado" });
            }

            if (!VerificarContrasenia(usuario, contraseniaActual))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new { isSuccess = false, message = "Contraseña actual incorrecta" });
            }

            usuario.Contrasenia = _passwordHasher.HashPassword(usuario, nuevaContrasenia);
            _bdpruebaProyectoContext.Usuarios.Update(usuario);
            await _bdpruebaProyectoContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, message = "Contraseña actualizada correctamente" });
        }

    }


}
