using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendEmail.Custom;
using SendEmail.Models;
using Microsoft.AspNetCore.Authorization;
using SendEmail.DTOs;
using APIsEstudioJuridico.Services;
using System.Security.Claims;
using System.Text.Json;
using APIsEstudioJuridico.Models;
using APIsEstudioJuridico.DTOs;
using APIsEstudioJuridico.Services.Contrato;


namespace SendEmail.Controllers
{
    [Route("api/[controller]")]
    //[Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    [Authorize(Roles = "Abogado,Administrador")]
    [ApiController]
    public class DocumentoController : ControllerBase
    {

        private readonly DocumentoService _documentoService;
        private readonly IOcrApiClient _ocrApiClient;
        private readonly ISupabaseStorageService _storageService;
        public DocumentoController(DocumentoService documentoService, IOcrApiClient ocrApiClient, ISupabaseStorageService storageService)
        {
            _documentoService = documentoService;
            _ocrApiClient = ocrApiClient;
            _storageService = storageService;
        }
        [HttpPost]
        [Route("Subir")]
        public async Task<IActionResult> Subir([FromForm] DocumentoDTO objeto)
        {
            try
            {
                if (objeto.Archivo == null)
                {
                    return BadRequest(new { mensaje = "No se ha enviado un archivo." });
                }

                var content = new MultipartFormDataContent();

                var fileContent = new StreamContent(objeto.Archivo.OpenReadStream());
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(objeto.Archivo.ContentType);

                content.Add(fileContent, "file", objeto.Archivo.FileName);

                var response = await _ocrApiClient.PostAsync(
                    "SubirArchivo",
                    content,
                    User,
                    OcrPermissions.Process,
                    OcrOperations.SubirArchivo,
                    HttpContext.RequestAborted);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    if ((int)response.StatusCode == StatusCodes.Status422UnprocessableEntity)
                    {
                        return StatusCode(
                            StatusCodes.Status422UnprocessableEntity,
                            new
                            {
                                mensaje = "No se pudo extraer texto del documento porque la calidad de la imagen es muy baja. Intente subir una imagen más nítida, con mejor iluminación y sin inclinación.",
                                detalle = errorContent
                            }
                        );
                    }

                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new
                        {
                            mensaje = $"Error al procesar el archivo: {response.ReasonPhrase}",
                            detalle = errorContent
                        }
                    );
                }

                var result = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(result);

                if (!json.RootElement.TryGetProperty("extracted_text", out var extractedText) ||
                    !json.RootElement.TryGetProperty("entities", out var entitiesJson))
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new { mensaje = "No se pudo extraer texto o entidades del archivo." }
                    );
                }

                string textoExtraido = extractedText.GetString() ?? string.Empty;

                List<IndiceRespuestaDTO> entidades =
                    JsonSerializer.Deserialize<List<IndiceRespuestaDTO>>(entitiesJson.GetRawText())
                    ?? new List<IndiceRespuestaDTO>();

                string? motorOCRUsado = null;

                if (json.RootElement.TryGetProperty("motorOCRUsado", out var motorOCRUsadoJson))
                {
                    motorOCRUsado = motorOCRUsadoJson.GetString();
                }

                List<string> recomendaciones = new();

                if (json.RootElement.TryGetProperty("recomendaciones", out var recomendacionesJson))
                {
                    recomendaciones =
                        JsonSerializer.Deserialize<List<string>>(recomendacionesJson.GetRawText())
                        ?? new List<string>();
                }

                objeto.ContenidoDocumento = textoExtraido;

                int idDocumento = await _documentoService.SubirDocumento(objeto, entidades, User);

                return Ok(new
                {
                    mensaje = "Archivo subido y procesado exitosamente",
                    idDocumento,
                    motorOCRUsado,
                    recomendaciones
                });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    new { mensaje = "No autorizado" }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { mensaje = ex.Message }
                );
            }
        }

        [HttpPost]
        [Route("SubirOCRAvanzado")]
        public async Task<IActionResult> SubirOCRAvanzado([FromForm] DocumentoDTO objeto)
        {
            try
            {
                if (objeto.Archivo == null)
                {
                    return BadRequest(new { mensaje = "No se ha enviado un archivo." });
                }

                var content = new MultipartFormDataContent();

                var fileContent = new StreamContent(objeto.Archivo.OpenReadStream());
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(objeto.Archivo.ContentType);

                content.Add(fileContent, "file", objeto.Archivo.FileName);

                var response = await _ocrApiClient.PostAsync(
                    "SubirArchivoPaddle",
                    content,
                    User,
                    OcrPermissions.Process,
                    OcrOperations.SubirArchivoPaddle,
                    HttpContext.RequestAborted);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    return StatusCode(
                        (int)response.StatusCode,
                        new
                        {
                            mensaje = "Error al procesar el archivo con extracción avanzada.",
                            detalle = errorContent
                        }
                    );
                }

                var result = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(result);

                if (!json.RootElement.TryGetProperty("extracted_text", out var extractedText) ||
                    !json.RootElement.TryGetProperty("entities", out var entitiesJson))
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new { mensaje = "No se pudo extraer texto o entidades del archivo." }
                    );
                }

                string textoExtraido = extractedText.GetString() ?? string.Empty;

                List<IndiceRespuestaDTO> entidades =
                    JsonSerializer.Deserialize<List<IndiceRespuestaDTO>>(entitiesJson.GetRawText())
                    ?? new List<IndiceRespuestaDTO>();

                string? motorOCRUsado = null;

                if (json.RootElement.TryGetProperty("motorOCRUsado", out var motorOCRUsadoJson))
                {
                    motorOCRUsado = motorOCRUsadoJson.GetString();
                }

                List<string> recomendaciones = new();

                if (json.RootElement.TryGetProperty("recomendaciones", out var recomendacionesJson))
                {
                    recomendaciones =
                        JsonSerializer.Deserialize<List<string>>(recomendacionesJson.GetRawText())
                        ?? new List<string>();
                }

                objeto.ContenidoDocumento = textoExtraido;

                int idDocumento = await _documentoService.SubirDocumento(objeto, entidades, User);

                return Ok(new
                {
                    mensaje = "Archivo subido y procesado exitosamente con extracción avanzada.",
                    idDocumento,
                    motorOCRUsado,
                    recomendaciones
                });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    new { mensaje = "No autorizado" }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { mensaje = ex.Message }
                );
            }
        }

        [HttpPost]
        [Route("SubirArchivoClasificado")]
        public async Task<IActionResult> SubirArchivoClasificado(
            [FromForm] ClasificarArchivoDTO solicitud)
        {
            var archivoRecibido = solicitud.File ?? solicitud.Archivo;

            if (archivoRecibido == null || archivoRecibido.Length == 0)
            {
                return BadRequest(new { detail = "No se ha enviado un archivo." });
            }

            try
            {
                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(archivoRecibido.OpenReadStream());
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(archivoRecibido.ContentType);

                content.Add(fileContent, "file", archivoRecibido.FileName);

                using var response = await _ocrApiClient.PostAsync(
                    "SubirArchivoClasificado",
                    content,
                    User,
                    OcrPermissions.Process,
                    OcrOperations.SubirArchivoClasificado,
                    HttpContext.RequestAborted);

                var responseContent = await response.Content.ReadAsStringAsync(
                    HttpContext.RequestAborted);

                return new ContentResult
                {
                    StatusCode = (int)response.StatusCode,
                    ContentType = response.Content.Headers.ContentType?.ToString()
                        ?? "application/json",
                    Content = responseContent
                };
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { detail = "No autorizado." });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { detail = "Error interno al solicitar la clasificación del archivo." });
            }
        }

        [HttpPost]
        [Route("ConfirmarPrediccion")]
        public async Task<IActionResult> ConfirmarPrediccion([FromBody] ConfirmarPrediccionDTO dto)
        {
            try
            {
                await _documentoService.ConfirmarPrediccionAsync(dto, User);
                return Ok(new { mensaje = "Predicción confirmada." });
            }
            catch (UnauthorizedAccessException) { return Unauthorized(new { mensaje = "No autorizado" }); }
            catch (KeyNotFoundException ex) { return NotFound(new { mensaje = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { mensaje = ex.Message }); }
        }

        [HttpGet("HistorialClasificacion/{idDoc}")]
        public async Task<IActionResult> HistorialClasificacion(int idDoc)
        {
            var historial = await _documentoService.ObtenerHistorialClasificacionAsync(idDoc);
            return Ok(new { value = historial });
        }

        [HttpPost]
        [Route("PrepararDataset")]
        public async Task<IActionResult> PrepararDataset([FromForm] ClasificarArchivoDTO solicitud, [FromQuery] string etiqueta)
        {
            var archivoRecibido = solicitud.File ?? solicitud.Archivo;
            if (archivoRecibido == null || archivoRecibido.Length == 0)
                return BadRequest(new { detail = "No se ha enviado un archivo." });
            if (string.IsNullOrWhiteSpace(etiqueta))
                return BadRequest(new { detail = "Debe proporcionar una etiqueta." });

            try
            {
                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(archivoRecibido.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(archivoRecibido.ContentType);
                content.Add(fileContent, "file", archivoRecibido.FileName);

                using var response = await _ocrApiClient.PostAsync(
                    $"PrepararDataset?etiqueta={Uri.EscapeDataString(etiqueta)}",
                    content,
                    User,
                    OcrPermissions.Admin,
                    OcrOperations.PrepararDataset,
                    HttpContext.RequestAborted);

                var responseContent = await response.Content.ReadAsStringAsync(HttpContext.RequestAborted);
                return new ContentResult
                {
                    StatusCode = (int)response.StatusCode,
                    ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json",
                    Content = responseContent
                };
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { detail = "No autorizado." });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { detail = "Error interno al preparar dataset." });
            }
        }

        [HttpPut]
        [Route("Actualizar/{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromForm] DocumentoDTO objeto)
        {
            try
            {
                var resultado = await _documentoService.ActualizarDocumentoConArchivo(id, objeto, User);

                return Ok(new { mensaje = resultado });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { mensaje = "No autorizado" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
        //[HttpPost]
        //[Route("SubirConClasificacion")]
        //public async Task<IActionResult> SubirConClasificacion([FromForm] DocumentoDTO objeto)
        //{
        //    try
        //    {
        //        if (objeto.Archivo == null)
        //        {
        //            return BadRequest(new { mensaje = "No se ha enviado un archivo." });
        //        }

        //        var content = new MultipartFormDataContent();
        //        var fileContent = new StreamContent(objeto.Archivo.OpenReadStream());
        //        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(objeto.Archivo.ContentType);
        //        content.Add(fileContent, "file", objeto.Archivo.FileName);

        //        string apiExtractionUrl = $"{_bePythonUrl}/SubirArchivo";
        //        var extractionResponse = await _httpClient.PostAsync(apiExtractionUrl, content);

        //        if (!extractionResponse.IsSuccessStatusCode)
        //        {
        //            return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = $"Error al procesar el archivo: {extractionResponse.ReasonPhrase}" });
        //        }

        //        var extractionResult = await extractionResponse.Content.ReadAsStringAsync();
        //        var extractionJson = JsonDocument.Parse(extractionResult);

        //        if (!extractionJson.RootElement.TryGetProperty("extracted_text", out var extractedText))
        //        {
        //            return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "No se pudo extraer el texto del archivo." });
        //        }

        //        string textoExtraido = extractedText.GetString();

        //        var classificationContent = new StringContent(JsonSerializer.Serialize(new { texto = textoExtraido }), Encoding.UTF8, "application/json");
        //        string apiClassificationUrl = $"{_bePythonUrl}/ClasificarDocumento";

        //        var classificationResponse = await _httpClient.PostAsync(apiClassificationUrl, classificationContent);

        //        if (!classificationResponse.IsSuccessStatusCode)
        //        {
        //            return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = $"Error al clasificar el documento: {classificationResponse.ReasonPhrase}" });
        //        }

        //        var classificationResult = await classificationResponse.Content.ReadAsStringAsync();
        //        var classificationJson = JsonDocument.Parse(classificationResult);

        //        if (!classificationJson.RootElement.TryGetProperty("tipo_documento", out var tipoDocumento))
        //        {
        //            return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "No se pudo clasificar el tipo de documento." });
        //        }

        //        objeto.ContenidoDocumento = textoExtraido;
        //        objeto.TipoDocumento = tipoDocumento.GetString();

        //        int idDocumento = await _documentoService.SubirDocumento(objeto, null, User);

        //        return Ok(new { mensaje = "Archivo subido, clasificado y procesado exitosamente", idDocumento, tipoDocumento = objeto.TipoDocumento });
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        return StatusCode(StatusCodes.Status401Unauthorized, new { mensaje = "No autorizado" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
        //    }
        //}
        [HttpGet("CambiarEstado/{id}")]
        public async Task<IActionResult> CambiarEstadoDocumento(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { mensaje = "El ID del documento no es válido." });
                }

                var documentoCambio = await _documentoService.CambiarEstado(id);
                if (documentoCambio == true)
                    return StatusCode(StatusCodes.Status200OK);
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
        [HttpGet("Detalles/{id}")]
        public async Task<IActionResult> ObtenerDetallesDocumento(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { mensaje = "El ID del documento no es válido." });
                }

                var documentoDetalle = await _documentoService.ObtenerDetallesDocumentoAsync(id);

                if (documentoDetalle == null)
                {
                    return NotFound(new { mensaje = "No se encontró el documento solicitado." });
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", response = documentoDetalle });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
        //[HttpPost]
        //[Route("SubirYProcesar")]
        //public async Task<IActionResult> SubirYProcesar([FromForm] DocumentoDTO objeto)
        //{
        //    try
        //    {
        //        if (objeto.Archivo == null || objeto.Archivo.Length == 0)
        //        {
        //            return BadRequest(new { mensaje = "Archivo no proporcionado o vacío." });
        //        }

        //        string resultado = await _documentoService.ProcesarYSubirDocumento(objeto, User);

        //        return Ok(new { mensaje = "Documento subido y procesado exitosamente", url = resultado });
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        return StatusCode(StatusCodes.Status401Unauthorized, new { mensaje = "No autorizado" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
        //    }
        //}
        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> ListaDocumentos()
        {
            List<DocumentoDTO> _lista = new List<DocumentoDTO>();
            try
            {
                _lista = await _documentoService.ListaDocumentos();
                if (_lista == null || !_lista.Any())
                {
                    return Ok(new { value = new List<DocumentoDTO>(), mensaje = "No se encontraron documentos." });
                }
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", response = _lista });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
        [HttpGet]
        [Route("Buscar/{id}")]
        public async Task<ActionResult<DocumentoDTO>> Get(int id)
        {
            try
            {
                var documentoDTO = await _documentoService.ObtenerDocumentoPorId(id);

                return Ok(documentoDTO);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
        [HttpGet]
        [Route("Obtener/{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var documento = await _documentoService.ObtenerDocumentoPorIdAsync(id);

                return Ok(new { value = documento });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
        [HttpGet("PorProceso/{idProceso}")]
        public async Task<IActionResult> ObtenerDocumentosPorProceso(int idProceso)
        {
            try
            {
                if (idProceso <= 0)
                {
                    return BadRequest(new { mensaje = "El ID del proceso no es válido." });
                }

                var documentos = await _documentoService.ObtenerDocumentosPorProceso(idProceso);

                if (documentos == null || !documentos.Any())
                {
                    return Ok(new { mensaje = "No se encontraron documentos para este proceso.", response = new List<DocumentoDTO>() });
                }

                return Ok(new { mensaje = "Documentos encontrados.", response = documentos });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
        [HttpGet("BuscarPorContenido")]
        public async Task<IActionResult> BuscarPorContenido(string query)
        {
            try
            {
                int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var idUsuario);
                var documentos = await _documentoService.BuscarDocumentosPorContenido(query, idUsuario > 0 ? idUsuario : null);
                if (documentos == null || !documentos.Any())
                {
                    return Ok(new { value = new List<DocumentoDTO>(), mensaje = "No se encontraron documentos." });
                }
                return Ok(new { value = documentos });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
        [HttpPost("BuscarPorFiltros")]
        public async Task<IActionResult> BuscarPorFiltros([FromBody] FiltroBusqueda filtro)
        {
            try
            {
                if (filtro == null)
                {
                    return BadRequest(new { mensaje = "El filtro de búsqueda no puede ser nulo." });
                }

                int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var idUsuario);
                var documentos = await _documentoService.BuscarDocumentosConFiltros(filtro, idUsuario > 0 ? idUsuario : null);

                if (documentos == null || !documentos.Any())
                {
                    return Ok(new { value = new List<DocumentoDTO>(), mensaje = "No se encontraron documentos." });
                }

                return Ok(new { value = documentos });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }

        [HttpGet("BuscarPorEntidad")]
        public async Task<IActionResult> BuscarPorEntidad([FromQuery] string? tipoEntidad, [FromQuery] string? valor)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoEntidad) && string.IsNullOrWhiteSpace(valor))
                    return BadRequest(new { mensaje = "Debe indicar tipoEntidad o valor." });

                int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var idUsuario);
                var documentos = await _documentoService.BuscarDocumentosPorEntidad(tipoEntidad, valor, idUsuario > 0 ? idUsuario : null);
                return Ok(new { value = documentos, mensaje = documentos.Any() ? "ok" : "No se encontraron documentos." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }
        [HttpGet("archivo-local")]
        public IActionResult ObtenerArchivoLocal([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return BadRequest(new { mensaje = "Ruta requerida." });
            // basic path traversal guard
            var normalized = path.Replace('\\', '/');
            if (normalized.Contains("..")) return BadRequest(new { mensaje = "Ruta inválida." });
            return ServirArchivoLocal(normalized);
        }

        private IActionResult ServirArchivoLocal(string relativePath)
        {
            var basePath = _storageService.LocalPath;
            var fullPath = Path.Combine(basePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var fullBase = Path.GetFullPath(basePath);
            var fullFile = Path.GetFullPath(fullPath);
            if (!fullFile.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullFile))
                return NotFound(new { mensaje = "Archivo no encontrado." });

            var ext = Path.GetExtension(fullFile).ToLowerInvariant();
            var contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                ".txt" => "text/plain",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };
            var fileName = Path.GetFileName(fullFile);
            return PhysicalFile(fullFile, contentType, fileName);
        }

        [HttpGet("{id}/url-firmada")]
        public async Task<IActionResult> ObtenerUrlFirmada(int id)
        {
            try
            {
                var url = await _documentoService.ObtenerUrlFirmadaDocumento(id, User);
                if (url.StartsWith("LOCAL:", StringComparison.OrdinalIgnoreCase))
                {
                    var localPath = url.Substring("LOCAL:".Length);
                    var fileUrl = $"{Request.Scheme}://{Request.Host}/api/Documento/archivo-local?path={Uri.EscapeDataString(localPath)}";
                    return Ok(new { url = fileUrl });
                }
                return Ok(new { url = url });

            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
        [HttpGet("{id}/abrir")]
        public async Task<IActionResult> AbrirDocumento(int id)
        {
            try
            {
                var url = await _documentoService.ObtenerUrlFirmadaDocumento(id, User);
                if (url.StartsWith("LOCAL:", StringComparison.OrdinalIgnoreCase))
                {
                    var localPath = url.Substring("LOCAL:".Length);
                    return ServirArchivoLocal(localPath);
                }
                return Redirect(url);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
        [HttpPut("{id}/reemplazar-archivo")]
        public async Task<IActionResult> ReemplazarArchivo(int id, [FromForm] IFormFile archivo)
        {
            try
            {
                var ruta = await _documentoService.ReemplazarArchivoEnMismaRuta(id, archivo, User);

                return Ok(new
                {
                    mensaje = "Archivo reemplazado correctamente en la misma ruta.",
                    ruta = ruta
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
