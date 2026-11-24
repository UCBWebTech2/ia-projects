using APIsEstudioJuridico.Services.Contrato;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SendEmail.Custom;

namespace SendEmail.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Abogado,Administrador")]
    [ApiController]
    public class ModeloController : ControllerBase
    {
        private readonly IOcrApiClient _ocrApiClient;

        public ModeloController(IOcrApiClient ocrApiClient)
        {
            _ocrApiClient = ocrApiClient;
        }

        [HttpGet]
        [Route("MetricasNaiveBayes")]
        public async Task<IActionResult> MetricasNaiveBayes(CancellationToken cancellationToken)
        {
            var response = await _ocrApiClient.GetAsync(
                "admin/model-metrics",
                User,
                OcrPermissions.Admin,
                OcrOperations.ModelMetrics,
                cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new
                {
                    mensaje = "No se pudieron obtener las métricas del modelo.",
                    detalle = body
                });
            }

            return Content(body, "application/json");
        }
    }
}