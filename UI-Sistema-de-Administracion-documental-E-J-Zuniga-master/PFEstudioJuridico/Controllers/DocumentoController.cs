using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PFEstudioJuridico.Models;
using System.Net.Http.Headers;
//using Firebase.Storage;
//using Firebase.Auth;
using PFEstudioJuridico.Models.DTOs;
using PFEstudioJuridico.ViewModels;
using PFEstudioJuridico.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using PFEstudioJuridico.Services;

namespace PFEstudioJuridico.Controllers
{
    [Authorize]
    public class DocumentoController : Controller
    {
        private readonly AuditoriaService _auditoriaService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DocumentoController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly string _beNetUrl;
        //private readonly FirebaseAuthClient _firebaseAuthClient;
        public DocumentoController(
            IHttpClientFactory httpClientFactory,
            AuditoriaService auditoriaService,
            IConfiguration configuration,
            ILogger<DocumentoController> logger,
            IWebHostEnvironment environment)
        {
            _httpClientFactory = httpClientFactory;
            //_firebaseAuthClient = firebaseAuthClient;
            _auditoriaService = auditoriaService;
            _logger = logger;
            _environment = environment;
            _beNetUrl = configuration["ApiSettings:BENET"]
                ?? throw new InvalidOperationException("No se encontró la URL NET en el servidor");
        }
        private string ObtenerMensajeErrorHttp(string operacion, HttpResponseMessage response, string contenidoError)
        {
            if (_environment.IsDevelopment())
            {
                return $"{operacion}. HTTP {(int)response.StatusCode} ({response.StatusCode}). Respuesta: {contenidoError}";
            }

            return $"{operacion}. Intente nuevamente.";
        }
        private string ObtenerMensajeErrorCalidad(
            HttpResponseMessage response,
            string errorContent,
            string mensajePredeterminado)
        {
            string mensajeUsuario;

            try
            {
                using var json = JsonDocument.Parse(errorContent);

                JsonElement detalle;

                if (json.RootElement.TryGetProperty("detail", out var detailElement))
                {
                    detalle = detailElement;
                }
                else
                {
                    detalle = json.RootElement;
                }

                var mensaje = mensajePredeterminado;
                string? recomendacion = null;
                string? accionSugerida = null;

                if (detalle.ValueKind == JsonValueKind.Object)
                {
                    if (detalle.TryGetProperty("mensaje", out var mensajeJson))
                    {
                        mensaje = mensajeJson.GetString() ?? mensaje;
                    }

                    if (detalle.TryGetProperty("recomendacion", out var recomendacionJson))
                    {
                        recomendacion = recomendacionJson.GetString();
                    }

                    if (detalle.TryGetProperty("accionSugerida", out var accionSugeridaJson))
                    {
                        accionSugerida = accionSugeridaJson.GetString();
                    }
                }

                if (string.Equals(accionSugerida, "USAR_OCR_AVANZADO", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(recomendacion))
                {
                    mensajeUsuario = $"{mensaje} {recomendacion}";
                }
                else
                {
                    mensajeUsuario = mensaje;
                }
            }
            catch
            {
                mensajeUsuario = mensajePredeterminado;
            }

            if (_environment.IsDevelopment())
            {
                return $"{mensajeUsuario} HTTP {(int)response.StatusCode} ({response.StatusCode}). Respuesta: {errorContent}";
            }

            return mensajeUsuario;
        }
        public async Task<IActionResult> Index()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Obtener documentos
                var responseDocumentos = await httpClient.GetAsync($"{_beNetUrl}/api/Documento/Lista");
                var documentos = new List<DocumentoDTO>();

                if (responseDocumentos.IsSuccessStatusCode)
                {
                    var jsonData = await responseDocumentos.Content.ReadAsStringAsync();
                    var apiResultado = JsonConvert.DeserializeObject<ResultadoAPI2>(jsonData);
                    documentos = apiResultado?.response ?? new List<DocumentoDTO>();
                }
                else
                {
                    ViewData["Mensaje"] = "Error al obtener los documentos. Código de estado: " + responseDocumentos.StatusCode;
                }

                // Obtener procesos
                var responseProcesos = await httpClient.GetAsync($"{_beNetUrl}/api/Proceso/ListaHabilitados");
                var procesos = new List<ProcesoDTO>();

                if (responseProcesos.IsSuccessStatusCode)
                {
                    var jsonDataProcesos = await responseProcesos.Content.ReadAsStringAsync();
                    procesos = JsonConvert.DeserializeObject<List<ProcesoDTO>>(jsonDataProcesos);
                }

                // Obtener categorías
                var responseCategorias = await httpClient.GetAsync($"{_beNetUrl}/api/Categoria/ListaHabilitadas");
                var categorias = new List<CategoriaDTO>();

                if (responseCategorias.IsSuccessStatusCode)
                {
                    var jsonDataCategorias = await responseCategorias.Content.ReadAsStringAsync();
                    categorias = JsonConvert.DeserializeObject<List<CategoriaDTO>>(jsonDataCategorias);
                }

                // Pasar los datos a la vista
                ViewBag.Documentos = documentos;
                ViewBag.Procesos = procesos;
                ViewBag.Categorias = categorias;

                return View(documentos);
            }

            ViewData["Mensaje"] = "No se ha proporcionado un token válido. Inicie sesión nuevamente.";
            return View(new List<DocumentoDTO>());
        }
        [HttpGet]
        public async Task<IActionResult> RegistroClasificado()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var responseProcesos = await httpClient.GetAsync($"{_beNetUrl}/api/Proceso/ListaHabilitados");
                var procesos = new List<ProcesoDTO>();

                if (responseProcesos.IsSuccessStatusCode)
                {
                    var jsonDataProcesos = await responseProcesos.Content.ReadAsStringAsync();
                    procesos = JsonConvert.DeserializeObject<List<ProcesoDTO>>(jsonDataProcesos);
                }

                // Obtener categorías
                var responseCategorias = await httpClient.GetAsync($"{_beNetUrl}/api/Categoria/ListaHabilitadas");
                var categorias = new List<CategoriaDTO>();

                if (responseCategorias.IsSuccessStatusCode)
                {
                    var jsonDataCategorias = await responseCategorias.Content.ReadAsStringAsync();
                    categorias = JsonConvert.DeserializeObject<List<CategoriaDTO>>(jsonDataCategorias);
                }
                // Pasar los datos a la vista
                ViewBag.Procesos = procesos;
                ViewBag.Categorias = categorias;

                return View();
            }

            ViewData["Mensaje"] = "No se ha proporcionado un token válido. Inicie sesión nuevamente.";
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegistrarDocumento(IFormFile archivo, string nombreDocumento, string tipoDocumento, int idProceso, bool habilitado)
        {
            // Validar archivo
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Archivo no válido.";
                return RedirectToAction("Index");
            }

            // Validar parámetros de entrada
            if (string.IsNullOrWhiteSpace(nombreDocumento) || string.IsNullOrWhiteSpace(tipoDocumento) || idProceso <= 0)
            {
                TempData["Error"] = "Parámetros inválidos.";
                return RedirectToAction("Index");
            }

            // Obtener token del claim
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Acceso");
            }

            // Crear cliente HTTP y configurar timeout
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var formData = new MultipartFormDataContent();

            try
            {
                // Leer el archivo y enviar al endpoint externo para clasificación
                using (var stream = archivo.OpenReadStream())
                {
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(archivo.ContentType);

                    var classificationFormData = new MultipartFormDataContent
            {
                { fileContent, "file", archivo.FileName }
            };

                    _logger.LogInformation(
                        "Clasificación documento: Archivo={Archivo}",
                        archivo.FileName);

                    var classificationResponse = await client.PostAsync(
                        $"{_beNetUrl}/api/Documento/SubirArchivoClasificado",
                        classificationFormData);
                    var classificationContent = await classificationResponse.Content.ReadAsStringAsync();

                    if (classificationResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation(
                            "Clasificación documento completada. Status={StatusCode}, Body={Body}",
                            classificationResponse.StatusCode,
                            classificationContent);
                    }
                    else
                    {
                        _logger.LogError(
                            "Error al clasificar documento. Status={StatusCode}, Body={Body}",
                            classificationResponse.StatusCode,
                            classificationContent);

                        if ((int)classificationResponse.StatusCode == StatusCodes.Status422UnprocessableEntity)
                        {
                            TempData["Warning"] = ObtenerMensajeErrorCalidad(
                                classificationResponse,
                                classificationContent,
                                "No se pudo clasificar el documento porque no se logró extraer texto con suficiente calidad.");
                            return RedirectToAction("Index");
                        }

                        TempData["Error"] = ObtenerMensajeErrorHttp(
                            "No se pudo clasificar el documento",
                            classificationResponse,
                            classificationContent);
                        return RedirectToAction("Index");
                    }

                    var classificationJson = JsonDocument.Parse(classificationContent);

                    // Obtener la categoría del documento (1 = Penal, 2 = Civil, 0 = Otro)
                    if (!classificationJson.RootElement.TryGetProperty("categoria", out var categoriaProperty))
                    {
                        TempData["Error"] = "No se pudo determinar la categoría del documento.";
                        return RedirectToAction("Index");
                    }

                    string? tipoClasificado = categoriaProperty.GetString();
                    if (string.IsNullOrWhiteSpace(tipoClasificado))
                    {
                        TempData["Error"] = "No se pudo determinar la categoría del documento.";
                        return RedirectToAction("Index");
                    }
                    int idCategoria;

                    switch (tipoClasificado?.ToLower())
                    {
                        case "penal":
                            idCategoria = 1;
                            break;
                        case "civil":
                            idCategoria = 2;
                            break;
                        default:
                            idCategoria = 0;
                            break;
                    }

                    if (idCategoria <= 0)
                    {
                        TempData["Error"] = $"La clasificación devolvió el tipo '{tipoClasificado}', pero no existe una categoría configurada para registrarlo.";
                        return RedirectToAction("Index");
                    }

                    // Agregar datos al formulario principal
                    formData.Add(new StringContent(nombreDocumento), "NombreDocumento");
                    formData.Add(new StringContent(tipoDocumento), "TipoDocumento");
                    formData.Add(new StringContent(idProceso.ToString()), "IdProceso");
                    formData.Add(new StringContent(idCategoria.ToString()), "IdCategoria");
                    formData.Add(new StringContent(habilitado.ToString()), "Habilitado");

                    _logger.LogInformation(
                        "Subida documento clasificado: Nombre={NombreDocumento}, Tipo={TipoDocumento}, Categoria={IdCategoria}, Proceso={IdProceso}, Habilitado={Habilitado}, Archivo={Archivo}",
                        nombreDocumento,
                        tipoDocumento,
                        idCategoria,
                        idProceso,
                        habilitado,
                        archivo.FileName);

                    // Leer el archivo nuevamente para la solicitud principal
                    using (var archivoStream = archivo.OpenReadStream())
                    {
                        var archivoContent = new StreamContent(archivoStream);
                        archivoContent.Headers.ContentType = new MediaTypeHeaderValue(archivo.ContentType);
                        formData.Add(archivoContent, "Archivo", archivo.FileName);

                        // Llamar al método Subir en la API
                        var subirResponse = await client.PostAsync($"{_beNetUrl}/api/Documento/Subir", formData);
                        var subirContent = await subirResponse.Content.ReadAsStringAsync();

                        if (subirResponse.IsSuccessStatusCode)
                        {
                            _logger.LogInformation(
                                "Subida de documento clasificado completada. Status={StatusCode}, Body={Body}",
                                subirResponse.StatusCode,
                                subirContent);
                            TempData["Success"] = "Documento registrado y subido exitosamente.";
                        }
                        else
                        {
                            _logger.LogError(
                                "Error al subir documento clasificado. Status={StatusCode}, Body={Body}",
                                subirResponse.StatusCode,
                                subirContent);

                            if ((int)subirResponse.StatusCode == StatusCodes.Status422UnprocessableEntity)
                            {
                                TempData["Warning"] = ObtenerMensajeErrorCalidad(
                                    subirResponse,
                                    subirContent,
                                    "No se pudo registrar el documento porque no se logró extraer texto con suficiente calidad.");
                            }
                            else
                            {
                                TempData["Error"] = ObtenerMensajeErrorHttp(
                                    "No se pudo registrar el documento",
                                    subirResponse,
                                    subirContent);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al clasificar y registrar el documento.");
                TempData["Error"] = "Ocurrió un error al procesar el documento. Intente nuevamente.";
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> SubirDocumento(IFormFile archivo, string nombreDocumento, string tipoDocumento, int? idProceso, int idCategoria, bool habilitado)
        {
            // Validar archivo
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Archivo no válido.";
                return RedirectToAction("Index");
            }

            // Validar tipos de archivo permitidos
            var tiposPermitidos = new[] { ".pdf", ".docx", ".doc", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(archivo.FileName).ToLower();
            if (!tiposPermitidos.Contains(extension))
            {
                TempData["Error"] = "Tipo de archivo no permitido. Solo se permiten archivos PDF, Word o imágenes (JPG, PNG).";
                return RedirectToAction("Index");
            }

            // Validar parámetros de entrada
            if (string.IsNullOrWhiteSpace(nombreDocumento) || string.IsNullOrWhiteSpace(tipoDocumento))
            {
                TempData["Error"] = "Parámetros inválidos.";
                return RedirectToAction("Index");
            }

            if (idCategoria <= 0)
            {
                TempData["Error"] = "Debe seleccionar una categoría.";
                return RedirectToAction("Index");
            }

            if (idProceso <= 0)
            {
                idProceso = null;
            }

            // Obtener token del claim
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Acceso");
            }

            // Crear cliente HTTP y configurar timeout y autorización
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var formData = new MultipartFormDataContent();

            // Agregar datos del formulario
            formData.Add(new StringContent(nombreDocumento), "NombreDocumento");
            formData.Add(new StringContent(tipoDocumento), "TipoDocumento");
            formData.Add(new StringContent(idCategoria.ToString()), "IdCategoria");
            formData.Add(new StringContent(habilitado.ToString()), "Habilitado");

            if (idProceso.HasValue)
            {
                formData.Add(new StringContent(idProceso.Value.ToString()), "IdProceso");
            }

            _logger.LogInformation(
                "Subida documento: Nombre={NombreDocumento}, Tipo={TipoDocumento}, Categoria={IdCategoria}, Proceso={IdProceso}, Habilitado={Habilitado}, Archivo={Archivo}",
                nombreDocumento,
                tipoDocumento,
                idCategoria,
                idProceso,
                habilitado,
                archivo.FileName);

            try
            {
                // Leer el archivo y agregar a la solicitud
                using (var stream = archivo.OpenReadStream())
                {
                    using (var fileContent = new StreamContent(stream))
                    {
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(archivo.ContentType);
                        formData.Add(fileContent, "Archivo", archivo.FileName);

                        // Enviar solicitud HTTP POST a la API
                        var response = await client.PostAsync($"{_beNetUrl}/api/Documento/Subir", formData);
                        var responseContent = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation(
                                "Subida documento completada. Status={StatusCode}, Body={Body}",
                                response.StatusCode,
                                responseContent);
                            TempData["Success"] = "Documento subido exitosamente.";
                        }
                        else
                        {
                            _logger.LogError(
                                "Error al subir documento. Status={StatusCode}, Body={Body}",
                                response.StatusCode,
                                responseContent);

                            if ((int)response.StatusCode == StatusCodes.Status422UnprocessableEntity)
                            {
                                TempData["Warning"] = ObtenerMensajeErrorCalidad(
                                    response,
                                    responseContent,
                                    "No se pudo subir el documento porque no se logró extraer texto con suficiente calidad.");
                            }
                            else
                            {
                                TempData["Error"] = ObtenerMensajeErrorHttp(
                                    "No se pudo subir el documento",
                                    response,
                                    responseContent);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al subir el documento.");
                TempData["Error"] = "Ocurrió un error al subir el documento. Intente nuevamente.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarDocumento(int id, IFormFile archivo, string nombreDocumento, string tipoDocumento, int idProceso, int idCategoria, bool habilitado)
        {
            if (archivo != null && archivo.Length == 0)
            {
                TempData["Error"] = "Archivo no válido.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(nombreDocumento) || string.IsNullOrWhiteSpace(tipoDocumento) || idProceso <= 0 || idCategoria <= 0)
            {
                TempData["Error"] = "Parámetros inválidos.";
                return RedirectToAction("Index");
            }

            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Acceso");
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(nombreDocumento), "NombreDocumento");
            formData.Add(new StringContent(tipoDocumento), "TipoDocumento");
            formData.Add(new StringContent(idProceso.ToString()), "IdProceso");
            formData.Add(new StringContent(idCategoria.ToString()), "IdCategoria");
            formData.Add(new StringContent(habilitado.ToString()), "Habilitado");

            if (archivo != null)
            {
                using (var stream = archivo.OpenReadStream())
                {
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(archivo.ContentType);
                    formData.Add(fileContent, "Archivo", archivo.FileName);
                }
            }
            try
            {
                var response = await client.PutAsync($"{_beNetUrl}/api/Documento/Actualizar/{id}", formData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Documento actualizado exitosamente.";
                }
                else
                {
                    TempData["Error"] = "No se pudo actualizar el documento. Intente nuevamente.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al actualizar el documento. Intente nuevamente.";
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Abrir(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Acceso");
            }

            var cliente = _httpClientFactory.CreateClient();
            cliente.BaseAddress = new Uri(_beNetUrl);
            cliente.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await cliente.GetAsync($"/api/Documento/{id}/abrir");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "No se pudo abrir el documento.");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();

            var contentType = response.Content.Headers.ContentType?.ToString()
                ?? "application/octet-stream";

            return File(bytes, contentType);
        }
        [HttpGet]
        public async Task<IActionResult> BusquedaAvanzada()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;


            ViewBag.Procesos = new List<ProcesoDTO>();
            ViewBag.Categorias = new List<CategoriaDTO>();

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Obtener procesos
                var responseProcesos = await httpClient.GetAsync($"{_beNetUrl}/api/Proceso/Lista");
                if (responseProcesos.IsSuccessStatusCode)
                {
                    var jsonDataProcesos = await responseProcesos.Content.ReadAsStringAsync();
                    ViewBag.Procesos = JsonConvert.DeserializeObject<List<ProcesoDTO>>(jsonDataProcesos) ?? new List<ProcesoDTO>();
                }

                // Obtener categorías
                var responseCategorias = await httpClient.GetAsync($"{_beNetUrl}/api/Categoria/Lista");
                if (responseCategorias.IsSuccessStatusCode)
                {
                    var jsonDataCategorias = await responseCategorias.Content.ReadAsStringAsync();
                    ViewBag.Categorias = JsonConvert.DeserializeObject<List<CategoriaDTO>>(jsonDataCategorias) ?? new List<CategoriaDTO>();
                }
            }
            else
            {
                ViewData["Mensaje"] = "No se ha proporcionado un token válido. Inicie sesión nuevamente.";
            }

            //return View();
            return View(new List<DocumentoDTO>());
        }
        [HttpPost]
        public async Task<IActionResult> BuscarPorFiltros(FiltroBusqueda filtro)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;

            List<DocumentoDTO> documentos = new List<DocumentoDTO>();
            ViewBag.Procesos = new List<ProcesoDTO>();
            ViewBag.Categorias = new List<CategoriaDTO>();

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Obtener procesos y categorías
                var responseProcesos = await httpClient.GetAsync($"{_beNetUrl}/api/Proceso/Lista");
                if (responseProcesos.IsSuccessStatusCode)
                {
                    var jsonDataProcesos = await responseProcesos.Content.ReadAsStringAsync();
                    ViewBag.Procesos = JsonConvert.DeserializeObject<List<ProcesoDTO>>(jsonDataProcesos) ?? new List<ProcesoDTO>();
                }

                var responseCategorias = await httpClient.GetAsync($"{_beNetUrl}/api/Categoria/Lista");
                if (responseCategorias.IsSuccessStatusCode)
                {
                    var jsonDataCategorias = await responseCategorias.Content.ReadAsStringAsync();
                    ViewBag.Categorias = JsonConvert.DeserializeObject<List<CategoriaDTO>>(jsonDataCategorias) ?? new List<CategoriaDTO>();
                }

                // Realizar la búsqueda de documentos
                var jsonFiltro = JsonConvert.SerializeObject(filtro);
                var content = new StringContent(jsonFiltro, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{_beNetUrl}/api/Documento/BuscarPorFiltros", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    var resultadoApi = JsonConvert.DeserializeObject<ResultadoAPI>(jsonData);
                    documentos = resultadoApi?.Value ?? new List<DocumentoDTO>();
                }
                else
                {
                    ViewData["Mensaje"] = $"Error al realizar la búsqueda. Código de estado: {response.StatusCode}";
                }
            }
            else
            {
                ViewData["Mensaje"] = "No se ha proporcionado un token válido. Inicie sesión nuevamente.";
            }

            return View("BusquedaAvanzada", documentos);
        }

        [HttpPost]
        public async Task<IActionResult> BuscarPorEntidad(string? tipoEntidad, string? valor)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;

            List<DocumentoDTO> documentos = new List<DocumentoDTO>();
            ViewBag.Procesos = new List<ProcesoDTO>();
            ViewBag.Categorias = new List<CategoriaDTO>();
            ViewBag.TipoEntidad = tipoEntidad;
            ViewBag.ValorEntidad = valor;

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var responseProcesos = await httpClient.GetAsync($"{_beNetUrl}/api/Proceso/Lista");
                if (responseProcesos.IsSuccessStatusCode)
                    ViewBag.Procesos = JsonConvert.DeserializeObject<List<ProcesoDTO>>(await responseProcesos.Content.ReadAsStringAsync()) ?? new List<ProcesoDTO>();
                var responseCategorias = await httpClient.GetAsync($"{_beNetUrl}/api/Categoria/Lista");
                if (responseCategorias.IsSuccessStatusCode)
                    ViewBag.Categorias = JsonConvert.DeserializeObject<List<CategoriaDTO>>(await responseCategorias.Content.ReadAsStringAsync()) ?? new List<CategoriaDTO>();

                var url = $"{_beNetUrl}/api/Documento/BuscarPorEntidad?tipoEntidad={Uri.EscapeDataString(tipoEntidad ?? "")}&valor={Uri.EscapeDataString(valor ?? "")}";
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    var resultadoApi = JsonConvert.DeserializeObject<ResultadoAPI>(jsonData);
                    documentos = resultadoApi?.Value ?? new List<DocumentoDTO>();
                }
                else
                {
                    ViewData["Mensaje"] = $"Error al buscar por entidad. Código de estado: {response.StatusCode}";
                }
            }
            else
            {
                ViewData["Mensaje"] = "No se ha proporcionado un token válido. Inicie sesión nuevamente.";
            }

            return View("BusquedaAvanzada", documentos);
        }
        [HttpGet]
        public async Task<IActionResult> DetallesDocumento(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "El documento no existe o el ID no es válido.";
                return RedirectToAction("Index");
            }
            var httpClient = _httpClientFactory.CreateClient();
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
            DocumentoDetalleDTO? documentoDetalle = null;
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Llamada a la API para obtener los detalles del documento
                var response = await httpClient.GetAsync($"{_beNetUrl}/api/Documento/Detalles/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();

                    // Deserializar la respuesta
                    //var jsonResponse = JsonConvert.DeserializeObject<dynamic>(jsonData);
                    var jsonResponse = JsonConvert.DeserializeObject<JObject>(jsonData);
                    var responseToken = jsonResponse?["response"];

                    //if (jsonResponse != null && jsonResponse.response != null)
                    //{
                    //    documentoDetalle = JsonConvert.DeserializeObject<DocumentoDetalleDTO>(jsonResponse.response.ToString());
                    //}
                    if (responseToken == null || responseToken.Type == JTokenType.Null)
                    {
                        TempData["Error"] = "No se pudo obtener la información del documento.";
                        return RedirectToAction("Index");
                    }
                    documentoDetalle = responseToken.ToObject<DocumentoDetalleDTO>();

                    if (documentoDetalle == null)
                    {
                        TempData["Error"] = "No se pudo convertir la información del documento.";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["Error"] = "No se pudo obtener la información del documento.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["Error"] = "No se proporcionó un token válido. Inicie sesión nuevamente.";
                return RedirectToAction("Index");
            }
            return View(documentoDetalle);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPrediccion(int idDocumento, int idCategoriaConfirmada)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "No se proporcionó un token válido. Inicie sesión nuevamente.";
                return RedirectToAction("DetallesDocumento", new { id = idDocumento });
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var payload = JsonConvert.SerializeObject(new { idDocumento, idCategoriaConfirmada });
            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{_beNetUrl}/api/Documento/ConfirmarPrediccion", content);
            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Predicción confirmada correctamente.";
            else
                TempData["Error"] = "No se pudo confirmar la predicción.";
            return RedirectToAction("DetallesDocumento", new { id = idDocumento });
        }

        [HttpGet]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "El documento no existe o el ID no es válido.";
                return RedirectToAction("Index");
            }

            var httpClient = _httpClientFactory.CreateClient();
            var token = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Cambia a GET para la llamada a la API
                var response = await httpClient.GetAsync($"{_beNetUrl}/api/Documento/CambiarEstado/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Estado cambiado exitosamente.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "No se pudo obtener la información del documento.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["Error"] = "No se proporcionó un token válido. Inicie sesión nuevamente.";
                return RedirectToAction("Index");
            }
        }
    }
}

