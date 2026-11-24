using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PFEstudioJuridico.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _beNetUrl;

        public DashboardController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _beNetUrl = configuration["ApiSettings:BENET"]
                ?? throw new InvalidOperationException("No se encontró la URL NET en el servidor");
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ModelMetrics = null;
            ViewBag.ModelMetricsError = null;
            ViewBag.DashboardResumen = null;
            ViewBag.DashboardError = null;

            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.ModelMetricsError = "No se encontró token de autenticación.";
                return View();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{_beNetUrl}/api/Modelo/MetricasNaiveBayes");
                var body = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(body);
                    ViewBag.ModelMetrics = doc.RootElement.Clone();
                }
                else
                {
                    ViewBag.ModelMetricsError = $"No se pudieron cargar métricas ({(int)response.StatusCode}).";
                }

                var res2 = await client.GetAsync($"{_beNetUrl}/api/Dashboard/Resumen");
                var body2 = await res2.Content.ReadAsStringAsync();
                if (res2.IsSuccessStatusCode)
                {
                    using var doc2 = JsonDocument.Parse(body2);
                    ViewBag.DashboardResumen = doc2.RootElement.Clone();
                }
                else
                {
                    ViewBag.DashboardError = $"No se pudieron cargar KPIs ({(int)res2.StatusCode}).";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ModelMetricsError = ViewBag.ModelMetricsError ?? ("Error al cargar métricas del modelo: " + ex.Message);
                ViewBag.DashboardError = "Error al cargar KPIs: " + ex.Message;
            }

            return View();
        }
    }
}