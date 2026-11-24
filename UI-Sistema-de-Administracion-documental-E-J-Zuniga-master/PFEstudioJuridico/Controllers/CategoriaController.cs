using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PFEstudioJuridico.DTOs;

namespace PFEstudioJuridico.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _beNetUrl;
        // Inyección de HttpClient
        public CategoriaController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _beNetUrl = configuration["ApiSettings:BENET"] ?? throw new InvalidOperationException("No se encontró la URL en el servidor");
        }

        private void AddBearerToken()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var apiUrl = $"{_beNetUrl}/api/Categoria/Lista";

            try
            {
                AddBearerToken();
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var categorias = JsonConvert.DeserializeObject<List<CategoriaDTO>>(content);
                    return View(categorias);
                }

                TempData["ErrorMessage"] = "No se pudo cargar la lista de categorías.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar categorías: {ex.Message}";
            }

            return View(new List<CategoriaDTO>());
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CategoriaDTO categoriaDTO)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Hay errores en el formulario. Por favor, corrige los datos.";
                return RedirectToAction("Index");
            }

            var idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;
            if (int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                categoriaDTO.IdUsuario = idUsuario;
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo obtener el ID del usuario.";
                return RedirectToAction("Index");
            }

            var apiUrl = $"{_beNetUrl}/api/Categoria/Crear";
            var jsonContent = new StringContent(JsonConvert.SerializeObject(categoriaDTO), Encoding.UTF8, "application/json");

            try
            {
                AddBearerToken();
                var response = await _httpClient.PostAsync(apiUrl, jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "La categoría se ha creado exitosamente.";
                    return RedirectToAction("Index");
                }

                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Error al crear la categoría: {errorResponse}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error de red al crear la categoría: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CategoriaDTO categoriaDTO)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Hay errores en el formulario. Por favor, corrige los datos.";
                return RedirectToAction("Index");
            }

            var apiUrl = $"{_beNetUrl}/api/Categoria/Actualizar/{categoriaDTO.IdCategoria}";
            var jsonContent = new StringContent(JsonConvert.SerializeObject(categoriaDTO), Encoding.UTF8, "application/json");

            try
            {
                AddBearerToken();
                var response = await _httpClient.PutAsync(apiUrl, jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "La categoría se ha actualizado exitosamente.";
                    return RedirectToAction("Index");
                }

                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Error al actualizar la categoría: {errorResponse}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error de red al actualizar la categoría: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "La categoría no existe o el ID no es válido.";
                return RedirectToAction("Index");
            }

            try
            {
                AddBearerToken();
                var response = await _httpClient.GetAsync($"{_beNetUrl}/api/Categoria/CambiarEstado/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Estado de la categoría cambiado exitosamente.";
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Error al cambiar el estado de la categoría: {errorResponse}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cambiar el estado de la categoría: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
