using Microsoft.AspNetCore.Mvc;

using PFEstudioJuridico.Data;
using PFEstudioJuridico.Models;
using Microsoft.EntityFrameworkCore;
using PFEstudioJuridico.ViewModels;
using Newtonsoft.Json;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PFEstudioJuridico.DTOs;
using System.Text;
using PFEstudioJuridico.Services;
using System.Net;
using PFEstudioJuridico.Models.DTOs;

namespace PFEstudioJuridico.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AppDBContext _appDbContext;
        private readonly HttpClient _httpClient;
        private readonly string _beNetUrl;
        //private readonly AuditoriaService _auditoriaService;
        public AccesoController(AppDBContext appDbContext, HttpClient httpClient, IConfiguration configuration)
        {
            _appDbContext = appDbContext;
            _httpClient = httpClient;
            _beNetUrl = configuration["ApiSettings:BENET"]
                ?? throw new InvalidOperationException ("No se encontró la URL NET en el servidor");

            //_auditoriaService = auditoriaService;
        }
        [HttpGet]
        public IActionResult Registrarse()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registrarse(UsuarioVM modelo)
        {
            if (modelo.clave != modelo.confirmarClave)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                return View();
            }

            var usuarioDTO = new UsuarioDTO
            {
                NombreUsuario = modelo.nombreUsuario,
                CorreoElectronico = modelo.correo,
                Contrasenia = modelo.clave,
                ConfirmarClave = modelo.confirmarClave,
                Rol = "Abogado",
                Restablecer = false,
                Confirmado = false
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_beNetUrl}/api/Acceso/Registrarse", usuarioDTO);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse>(responseData);
                    if(result== null)
                    {
                        ViewData["Mensaje"] = "Error, la respuesta del servidor no tiene un formato válido.";
                        return View(usuarioDTO);
                    }
                    if (result.isSuccess)
                    {
                        return RedirectToAction("Login", "Acceso");
                    }
                    else
                    {
                        ViewData["Mensaje"] = result.message ?? "No se pudo registrar al usuario";
                    }
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    var errorResult = JsonConvert.DeserializeObject<ApiResponse>(errorData);
                    ViewData["Mensaje"] = errorResult?.message ?? "Error, no se pudo registrar al usuario";
                }
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = $"Error, no se pudo registrar al usuario: {ex.Message}";
            }

            return View();
        }
        public IActionResult AccesoDenegado()
        {
            return View();
        }
        public async Task<IActionResult> Confirmar(string token)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_beNetUrl}/api/Usuario/ObtenerUsuario?token={token}");
                if (response.IsSuccessStatusCode)
                {
                    var usuarioExiste = await response.Content.ReadFromJsonAsync<bool>();
                    ViewBag.Respuesta = usuarioExiste;
                }
                else
                {
                    ViewBag.Respuesta = false;
                    throw new Exception("Error al consumir la API");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Respuesta = false;
                ViewBag.Error = $"Error: {ex.Message}";
            }

            //ViewBag.IdUsuario = idUsuario;
            return View();
        }
        public async Task<IActionResult> Confirmar2(string token)
        {
            try
                {
                if (string.IsNullOrEmpty(token))
                {
                    ViewBag.Respuesta = false;
                    ViewBag.Mensaje = "El token es obligatorio.";
                    return View("Confirmar");
                }

                // Llamada para obtener el correo asociado al token
                var correoResponse = await _httpClient.GetAsync($"{_beNetUrl}/api/Usuario/ObtenerCorreoPorToken?token={token}");
                if (correoResponse.IsSuccessStatusCode)
                {
                    var correo = await correoResponse.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(correo))
                    {
                        // Llamar al servicio de Restablecer con el correo
                        var restablecerResponse = await _httpClient.PostAsync($"{_beNetUrl}/api/Acceso/Restablecer?correo={correo}", null);
                        if (restablecerResponse.IsSuccessStatusCode)
                        {
                            var apiResponse = await restablecerResponse.Content.ReadFromJsonAsync<ApiRespuesta>();
                            ViewBag.Respuesta = apiResponse?.Restablecido ?? false;
                            ViewBag.Mensaje = apiResponse?.Mensaje;
                        }
                        else
                        {
                            throw new Exception("Error al consumir la API de Restablecer.");
                        }
                    }
                    else
                    {
                        ViewBag.Respuesta = false;
                        ViewBag.Mensaje = "No se encontró un usuario asociado al token proporcionado.";
                    }
                }
                else if (correoResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.Respuesta = false;
                    ViewBag.Mensaje = "El token no es válido o no tiene un usuario asociado.";
                }
                else
                {
                    throw new Exception("Error al consumir la API de ObtenerCorreoPorToken.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Respuesta = false;
                ViewBag.Error = $"Error: {ex.Message}";
            }

            return View("Confirmar");
        }

        [HttpGet]
        public IActionResult Restablecer()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RestablecerCuenta(string correo)
        {
            try
            {
                // Llamar al método externo para restablecer la cuenta
                var response = await _httpClient.PostAsJsonAsync($"{_beNetUrl}/api/Acceso/Restablecer", new { correo });

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiRespuesta>();
                    return Ok(apiResponse);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { Restablecido = false, Mensaje = $"Error al consumir la API: {errorContent}" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Restablecido = false, Mensaje = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Restablecer(string correo)
        {
            try
            {
                // Validar correo
                if (string.IsNullOrEmpty(correo))
                {
                    ViewBag.Respuesta = false;
                    ViewBag.Error = "Debe proporcionar una dirección de correo válida.";
                    return View();
                }

                // Llamar al método externo para restablecer la cuenta
                var response = await _httpClient.PostAsync($"{_beNetUrl}/api/Acceso/Restablecer?correo={correo}", null);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiRespuesta>();
                    ViewBag.Respuesta = apiResponse?.Restablecido ?? false;
                    ViewBag.Mensaje = apiResponse?.Mensaje;
                }
                else
                {
                    ViewBag.Respuesta = false;
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al consumir la API: {response.StatusCode} - {response.ReasonPhrase}. Detalles: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Respuesta = false;
                ViewBag.Error = $"Error: {ex.Message}";
            }

            ViewBag.Correo = correo;
            return View();
        }





        [HttpGet]
        public IActionResult Actualizar(string token)
        {
            ViewBag.Token = token;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Actualizar(string token, string clave, string confirmarClave)
        {

            if (clave != confirmarClave)
            {
                ViewBag.Mensaje = "Las contraseñas no coinciden";
                return View();
            }
            if (clave.Length < 8)
            {
                ViewBag.Mensaje = "La contraseña debe tener al menos 8 caracteres.";
                return View();
            }

            var actualizarContraseniaDTO = new ActualizarContraseniaDTO
            {
                NuevaContrasenia = clave,
                ConfirmarClave = confirmarClave,
                Token = token
            };

            try
            {
                var contenido = new StringContent(JsonConvert.SerializeObject(actualizarContraseniaDTO), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_beNetUrl}/api/Acceso/ActualizarContraseniaDesdeApi", contenido);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Restablecido = true;
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    var errorResult = JsonConvert.DeserializeObject<ApiResponse>(errorData);
                    ViewBag.Mensaje = errorResult?.message ?? "Error, no se pudo actualizar la contraseña";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = $"Error, no se pudo actualizar la contraseña: {ex.Message}";
            }

            return View();
        }


        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM modelo)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Mensaje"] = "Por favor, corrija los errores en el formulario.";
                return View(modelo);
            }

            // Validación adicional del lado del servidor
            if (string.IsNullOrWhiteSpace(modelo.correo) || string.IsNullOrWhiteSpace(modelo.clave))
            {
                ViewData["Mensaje"] = "El correo y la contraseña son obligatorios.";
                return View(modelo);
            }
            // Sanitizar la entrada para prevenir inyecciones
            modelo.correo = modelo.correo.Trim();
            modelo.clave = modelo.clave.Trim();
            using var cliente = new HttpClient();
            var contenido = new StringContent(
                JsonConvert.SerializeObject(new
                {
                    CorreoElectronico = modelo.correo,
                    Contrasenia = modelo.clave
                }),
                System.Text.Encoding.UTF8, "application/json"
            );

            try
            {
                var response = await cliente.PostAsync($"{_beNetUrl}/api/Acceso/Login", contenido);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var data = JsonConvert.DeserializeObject<LoginApiResponse>(jsonResponse);
                if (!response.IsSuccessStatusCode || data == null || !data.IsSuccess)
                {
                    ViewData["Mensaje"] =
                        data?.Message
                        ?? "No se pudo iniciar sesión.";
                    return View(modelo);
                }
                if(string.IsNullOrWhiteSpace(data.Token)|| data.Usuario == null)
                {
                    ViewData["Mensaje"] = "La respuesta del servidor de autenticación no es válida.";
                    return View(modelo);
                }
                var usuario = data.Usuario;

                if(usuario.IdUsuario <= 0 || string.IsNullOrWhiteSpace(usuario.NombreUsuario) || string.IsNullOrWhiteSpace(usuario.Rol))
                {
                    ViewData["Mensaje"] = "Los datos del usuario recibidos no son válidos.";
                    return View(modelo);
                }
                string tokenJWT = data.Token;
                string tokenRestablecimiento = usuario.Token ?? string.Empty;

                List<Claim> claims = new List<Claim>
                {

                    new Claim(ClaimTypes.Name, modelo.correo),
                    new Claim("Token", tokenJWT),
                    new Claim("TokenR", tokenRestablecimiento),
                    new Claim("IdUsuario", usuario.IdUsuario.ToString()),
                    new Claim("NombreUsuario", usuario.NombreUsuario),
                    new Claim(ClaimTypes.Role, usuario.Rol)
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var properties = new AuthenticationProperties
                {
                    AllowRefresh = true
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                var auditoriaDTO = new
                {
                    IdUsuario = usuario.IdUsuario,
                    Accion = "Inicio de sesión",
                    Descripcion = $"El usuario {modelo.correo} ha iniciado sesión."
                };

                using var auditoriaCliente = new HttpClient();
                auditoriaCliente.BaseAddress = new Uri(_beNetUrl);

                var auditoriaContenido = new StringContent(
                    JsonConvert.SerializeObject(auditoriaDTO),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var auditoriaResponse = await auditoriaCliente.PostAsync(
                    "/api/Auditoria/Registrar",
                    auditoriaContenido
                );

                if (!auditoriaResponse.IsSuccessStatusCode)
                {
                    ViewData["MensajeAuditoria"] = "Error al registrar la acción de auditoría.";
                }

                return RedirectToAction("Index", "Home");
            }
            catch (JsonException)
            {
                ViewData["Mensaje"] = "La respuesta del servidor no tiene un formato válido.";
                return View(modelo);
            }
            catch (HttpRequestException)
            {
                ViewData["Mensaje"] = "No se pudo conectar con el servidor de autenticación.";
                return View(modelo);
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = "Ocurrió un error: " + ex.Message;
                return View(modelo);
            }
        }


        public async Task<IActionResult> CambiarContrasenia(ActualizarContraseniaDTO modelo)
        {
            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Datos inválidos.";
                return RedirectToAction("Perfil", "Home");
            }

            var token = User.Claims.FirstOrDefault(c => c.Type == "TokenR")?.Value;
            if (token == null)
            {
                TempData["Mensaje"] = "No se encontró el token de restablecimiento.";
                return RedirectToAction("Perfil", "Home");
            }

            modelo.Token = token;
            var cliente = new HttpClient();
            var contenido = new StringContent(JsonConvert.SerializeObject(modelo), System.Text.Encoding.UTF8, "application/json");
            var response = await cliente.PostAsync($"{_beNetUrl}/api/Acceso/ActualizarContrasenia", contenido);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Clave cambiada exitosamente.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction("Perfil", "Home");
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            var resultado = JsonConvert.DeserializeObject<ResultadoAPI2>(errorResponse);
            //TempData["Mensaje"] = errorResponse;
            TempData["Mensaje"] = resultado?.mensaje ?? "Error, no se pudo cambiar la clave.";
            TempData["TipoMensaje"] = "error";
            return RedirectToAction("Perfil", "Home");
        }


        //Metodo para cerrar sesion
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Acceso");
        }
    }
}
