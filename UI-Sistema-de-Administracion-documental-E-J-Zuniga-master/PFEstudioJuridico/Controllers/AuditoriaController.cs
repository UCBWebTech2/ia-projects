using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PFEstudioJuridico.DTOs;
using PFEstudioJuridico.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PFEstudioJuridico.Controllers
{
    public class AuditoriaController : Controller
    {
        //private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _beNetUrl;
        public AuditoriaController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _beNetUrl = configuration["ApiSettings:BENET"] ?? throw new InvalidOperationException("No se encontró la URL NET en el servidor");
        }
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Llamada a la API para obtener las auditorías
                var response = await httpClient.GetAsync($"{_beNetUrl}/api/Auditoria/Lista");
                var auditorias = new List<AuditoriaDTO>();

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<RespuestaApiAuditoria<List<AuditoriaDTO>>>(jsonData);
                    if (apiResponse != null && apiResponse.Response != null)
                    {
                        auditorias = apiResponse.Response;
                    }
                }
                else
                {
                    ViewData["Mensaje"] = "Error al obtener los registros de auditoría. Código de estado: " + response.StatusCode;
                }

                return View(auditorias);
            }

            ViewData["Mensaje"] = "No se ha proporcionado un token válido. Inicie sesión nuevamente.";
            return View(new List<AuditoriaDTO>());
        }
    }
}
