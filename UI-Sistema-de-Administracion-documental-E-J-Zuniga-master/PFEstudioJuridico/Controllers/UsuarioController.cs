using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PFEstudioJuridico.DTOs;
using PFEstudioJuridico.Models;
using Microsoft.AspNetCore.Authorization;

namespace PFEstudioJuridico.Controllers
{
    //[Authorize]
    public class UsuarioController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _beNetUrl;

        public UsuarioController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _beNetUrl = configuration["ApiSettings:BENET"] ?? throw new InvalidOperationException("No se encontró la URL en el servidor");
        }
        [Authorize(Roles="Administrador")]
        public async Task<IActionResult> Index()
        {
            var apiUrl = $"{_beNetUrl}/api/Usuario/Lista";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var usuarios = JsonConvert.DeserializeObject<List<UsuarioDTO2>>(content);
                return View(usuarios);
            }

            return View(new List<Usuario>());
        }
        [HttpPost]
        [Route("CambiarEstadoConfirmado")]
        public async Task<IActionResult> CambiarEstadoConfirmado(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "El ID del usuario no es válido.";
                    return RedirectToAction("Index");
                }

                // Llamar al endpoint de la API
                var apiUrl = $"{_beNetUrl}/api/Usuario/CambiarEstadoConfirmado/{id}";
                var response = await _httpClient.PutAsync(apiUrl, null); // PUT con cuerpo vacío

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Estado de confirmación del usuario cambiado exitosamente.";
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Error al cambiar el estado del usuario: {errorResponse}";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocurrió un error inesperado: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Route("RegistrarUsuario")]
        public async Task<IActionResult> RegistrarUsuario(RegistroUsuarioDTO rUsuarioDTO)
        {
            try
            {
                if (rUsuarioDTO.Rol == "Seleccione")
                {
                    ModelState.AddModelError("Rol", "Debe seleccionar un rol válido.");
                }
                //if (!ModelState.IsValid)
                //{
                //    var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                //    TempData["ErrorMessage"] = $"Errores en el formulario: {string.Join(", ", errores)}";
                //    return RedirectToAction("Index");
                //}

                // Preparar la URL del endpoint de la API
                var apiUrl = $"{_beNetUrl}/api/Usuario/RegistrarUsuario";

                // Serializar los datos a JSON
                var jsonContent = JsonConvert.SerializeObject(rUsuarioDTO);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Enviar la solicitud POST a la API
                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Usuario registrado exitosamente. Se ha enviado un correo de restablecimiento.";
                    return RedirectToAction("Index");
                }
                else
                {
                    // Leer la respuesta de error de la API
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Error al registrar el usuario: {errorResponse}";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                TempData["ErrorMessage"] = $"Ocurrió un error inesperado: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [Route("ActualizarUsuario")]
        public async Task<IActionResult> ActualizarUsuario(ActualizarUsuarioDTO actualizarUsuarioDTO)
        {
            try
            {
                // Preparar la URL del endpoint de la API
                var apiUrl = $"{_beNetUrl}/api/Usuario/ActualizarUsuario";

                // Serializar los datos a JSON
                var jsonContent = JsonConvert.SerializeObject(actualizarUsuarioDTO);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Enviar la solicitud PUT a la API
                var response = await _httpClient.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
                    return RedirectToAction("Index"); // Redirigir a la lista de usuarios
                }
                else
                {
                    // Leer la respuesta de error de la API
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Error al actualizar el usuario: {errorResponse}";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                TempData["ErrorMessage"] = $"Ocurrió un error inesperado: {ex.Message}";
                return RedirectToAction("Index");
            }
        }


        //[HttpPost]
        //[Route("RegistrarUsuario")]
        //public async Task<IActionResult> RegistrarUsuario(RegistroUsuarioDTO rusuarioDTO)
        //{
        //    //if (!ModelState.IsValid)
        //    //{
        //    //    TempData["ErrorMessage"] = "Hay errores en el formulario. Por favor, corrige los datos.";
        //    //    return RedirectToAction("Index");
        //    //}
        //    // Lógica para generar contraseña y token se puede mover aquí primero
        //    // Genera una contraseña aleatoria y el token antes de la solicitud
        //    //usuarioDTO.Contrasenia = Utilidades.GenerateRandomPassword();
        //    //usuarioDTO.Contrasenia = Utilidades.EncriptarClave(usuarioDTO.Contrasenia);
        //    //usuarioDTO.Token = Utilidades.GenerarToken();
        //    var idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;

        //    var apiUrl = $"{_beNetUrl}/api/Usuario/RegistroUsuario";
        //    var jsonContent = JsonConvert.SerializeObject(rusuarioDTO);
        //    var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        //    var response = await _httpClient.PostAsync(apiUrl, content);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        return RedirectToAction("Index");
        //    }

        //    // Manejo de errores
        //    var errorResponse = await response.Content.ReadAsStringAsync();
        //    return BadRequest(new { Message = "Error al registrar el usuario.", Details = errorResponse });
        //}
        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
