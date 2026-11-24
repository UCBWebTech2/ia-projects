using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PFEstudioJuridico.DTOs;
using PFEstudioJuridico.Models.DTOs;
using PFEstudioJuridico.Services;
using PFEstudioJuridico.ViewModels;

namespace PFEstudioJuridico.Controllers
{
    [Authorize]
    public class ProcesoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _beNetUrl;
        public ProcesoController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _beNetUrl = configuration["ApiSettings:BENET"]
                ?? throw new InvalidOperationException("No se encontró la URL en el servidor");
        }

        private void AddBearerToken()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<IActionResult> Index()
        {
            AddBearerToken();
            var apiUrl = $"{_beNetUrl}/api/Proceso/Lista";

            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var procesos = JsonConvert.DeserializeObject<List<ProcesoDTO>>(content);
                return View(procesos);
            }

            TempData["Error"] = "No se pudo obtener la lista de procesos.";
            return View(new List<ProcesoDTO>());
        }

        [HttpGet]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "El proceso no existe o el ID no es válido.";
                return RedirectToAction("Index");
            }

            AddBearerToken();
            var response = await _httpClient.GetAsync($"{_beNetUrl}/api/Proceso/CambiarEstado/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Estado del proceso cambiado exitosamente.";
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Error al cambiar el estado del proceso: {errorResponse}";
            }

            return RedirectToAction("Index");
        }

        public IActionResult NuevoProceso()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(ProcesoDTO procesoDTO)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Hay errores en el formulario. Por favor, corrige los datos.";
                return View("Index");
            }

            var idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;
            if (int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                procesoDTO.IdUsuario = idUsuario;
            }
            else
            {
                TempData["ErrorMessage"] = "El IdUsuario no se pudo obtener.";
                return RedirectToAction("Index");
            }

            AddBearerToken();
            var apiUrl = $"{_beNetUrl}/api/Proceso/Crear";
            var jsonContent = new StringContent(JsonConvert.SerializeObject(procesoDTO), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "El proceso se ha creado exitosamente.";
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Error al crear el proceso: {errorResponse}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Editar(ProcesoDTO procesoDTO)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Hay errores en el formulario. Por favor, corrige los datos.";
                return RedirectToAction("Index");
            }

            AddBearerToken();
            var apiUrl = $"{_beNetUrl}/api/Proceso/Actualizar/{procesoDTO.IdProceso}";
            var jsonContent = new StringContent(JsonConvert.SerializeObject(procesoDTO), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "El proceso se ha actualizado exitosamente.";
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Error al actualizar el proceso: {errorResponse}";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> HistorialProcesos(int idProceso)
        {
            AddBearerToken();

            var responseDocumentos = await _httpClient.GetAsync($"{_beNetUrl}/api/Documento/PorProceso/{idProceso}");
            var documentos = new List<DocumentoDTO>();

            if (responseDocumentos.IsSuccessStatusCode)
            {
                var jsonData = await responseDocumentos.Content.ReadAsStringAsync();
                var apiResultado = JsonConvert.DeserializeObject<ResultadoAPI2>(jsonData);
                documentos = apiResultado?.response ?? new List<DocumentoDTO>();
            }
            else
            {
                TempData["Error"] = "Error al obtener los documentos relacionados al proceso.";
            }

            ViewBag.IdProceso = idProceso;
            return View(documentos);
        }
    }
}
