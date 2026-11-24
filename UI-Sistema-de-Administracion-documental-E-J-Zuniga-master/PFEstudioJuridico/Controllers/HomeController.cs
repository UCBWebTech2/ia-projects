using Microsoft.AspNetCore.Mvc;
using PFEstudioJuridico.Models;
using System.Diagnostics;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using PFEstudioJuridico.ViewModels;
using System.Security.Claims;
using System.Text.Json;
using PFEstudioJuridico.DTOs;
using PFEstudioJuridico.Models.DTOs;

namespace PFEstudioJuridico.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _beNetUrl;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _beNetUrl = configuration["ApiSettings:BENET"] ?? throw new InvalidOperationException("No se encontr� la URL NET en el servidor");
        }

        public async Task<IActionResult> Index()
        {
            var tokenJWT = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
            int totalDocumentos = 0, totalProcesos = 0, totalUsuarios = 0, totalCategorias = 0;
            var recientes = new List<Models.DTOs.DocumentoDTO>();

            if (!string.IsNullOrEmpty(tokenJWT))
            {
                try
                {
                    using var cliente = new HttpClient();
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenJWT);

                    async Task<JsonDocument?> GetJson(string url)
                    {
                        var resp = await cliente.GetAsync(url);
                        if (!resp.IsSuccessStatusCode) return null;
                        var s = await resp.Content.ReadAsStringAsync();
                        return JsonDocument.Parse(s);
                    }

                    var docTask = GetJson($"{_beNetUrl}/api/Documento/Lista");
                    var procTask = GetJson($"{_beNetUrl}/api/Proceso/Lista");
                    var usrTask = GetJson($"{_beNetUrl}/api/Usuario/Lista");
                    var catTask = GetJson($"{_beNetUrl}/api/Categoria/Lista");

                    await Task.WhenAll(docTask, procTask, usrTask, catTask);

                    JsonElement GetResponseArray(JsonDocument? jd)
                    {
                        if (jd == null) return default;
                        if (jd.RootElement.ValueKind == JsonValueKind.Array) return jd.RootElement;
                        if (jd.RootElement.TryGetProperty("response", out var r) && r.ValueKind == JsonValueKind.Array) return r;
                        return default;
                    }

                    var docArr = GetResponseArray(docTask.Result);
                    var procArr = GetResponseArray(procTask.Result);
                    var usrArr = GetResponseArray(usrTask.Result);
                    var catArr = GetResponseArray(catTask.Result);

                    if (docArr.ValueKind == JsonValueKind.Array)
                    {
                        totalDocumentos = docArr.GetArrayLength();
                        foreach (var item in docArr.EnumerateArray().Take(5))
                        {
                            recientes.Add(new Models.DTOs.DocumentoDTO
                            {
                                IdDoc = item.TryGetProperty("idDoc", out var id) ? id.GetInt32() : 0,
                                NombreDocumento = item.TryGetProperty("nombreDocumento", out var n) ? n.GetString() : "",
                                TipoDocumento = item.TryGetProperty("tipoDocumento", out var t) ? t.GetString() : "",
                                NombreCategoria = item.TryGetProperty("nombreCategoria", out var c) ? c.GetString() : "",
                                NombreProceso = item.TryGetProperty("nombreProceso", out var p) ? p.GetString() : "",
                                FechaCreacion = item.TryGetProperty("fechaCreacion", out var f) && f.ValueKind != JsonValueKind.Null ? f.GetDateTime() : null
                            });
                        }
                    }
                    if (procArr.ValueKind == JsonValueKind.Array) totalProcesos = procArr.GetArrayLength();
                    if (usrArr.ValueKind == JsonValueKind.Array) totalUsuarios = usrArr.GetArrayLength();
                    if (catArr.ValueKind == JsonValueKind.Array) totalCategorias = catArr.GetArrayLength();

                    docTask.Result?.Dispose(); procTask.Result?.Dispose(); usrTask.Result?.Dispose(); catTask.Result?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudieron cargar estadísticas del dashboard");
                }
            }

            ViewData["TotalDocumentos"] = totalDocumentos;
            ViewData["TotalProcesos"] = totalProcesos;
            ViewData["TotalUsuarios"] = totalUsuarios;
            ViewData["TotalCategorias"] = totalCategorias;
            ViewData["DocumentosRecientes"] = recientes;
            return View();
        }
        //public IActionResult Index()
        //{
            
        //}

        private Usuario? ObtenerUsuarioDesdeClaims()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
            {
                return null;
            }

            //var claims = claimsIdentity.Claims;
            //return new Usuario
            //{
            //    idUsuario = Convert.ToInt32(claims.FirstOrDefault(c => c.Type == "IdUsuario")?.Value),
            //    nombreUsuario = claims.FirstOrDefault(c => c.Type == "NombreUsuario")?.Value,
            //    correoElectronico = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
            //    Rol = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
            //};
            var idUsuarioClaim = claimsIdentity.FindFirst("IdUsuario")?.Value;
            int.TryParse(idUsuarioClaim, out int idUsuario);

            return new Usuario
            {
                idUsuario = idUsuario,
                nombreUsuario = claimsIdentity.FindFirst("NombreUsuario")?.Value ?? string.Empty,
                correoElectronico = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                Rol = claimsIdentity.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty
            };
        }

        [HttpPost]
        public async Task<IActionResult> Subir_Archivo(string nombreDocumento, string tipoDocumento, IFormFile documento)
        {
            var cliente = new HttpClient();
            var tokenJWT = User.Claims.FirstOrDefault(c => c.Type == "Token")?.Value;
            if (tokenJWT == null)
            {
                // Si no se encuentra el token, redirigir o mostrar un mensaje de error
                ViewData["Mensaje"] = "No se encontr� el token de autenticaci�n.";
                return View("Index");
            }
            // Agrega el token JWT al encabezado de autorizaci�n
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenJWT);

            using (var multipartFormContent = new MultipartFormDataContent())
            {
                multipartFormContent.Add(new StringContent(nombreDocumento), name: "NombreDocumento");
                multipartFormContent.Add(new StringContent(tipoDocumento), name: "TipoDocumento");
                
                var filestreamContent = new StreamContent(documento.OpenReadStream());
                filestreamContent.Headers.ContentType = new MediaTypeHeaderValue(documento.ContentType);

                multipartFormContent.Add(filestreamContent, name: "Archivo", fileName: documento.FileName);

                //var response = await cliente.PostAsync("{_beNetUrl}/api/Documento/Subir", multipartFormContent);
                // Realiza la solicitud POST con el token JWT en los encabezados
                var response = await cliente.PostAsync($"{_beNetUrl}/api/Documento/Subir", multipartFormContent);

                if (response.IsSuccessStatusCode)
                {
                    var test = await response.Content.ReadAsStringAsync();
                    ViewData["Mensaje"] = "Archivo subido exitosamente.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    if ((int)response.StatusCode == StatusCodes.Status422UnprocessableEntity)
                    {
                        ViewData["Mensaje"] = "No se pudo subir el documento porque no se logr� extraer texto con suficiente calidad. Intente subir una imagen m�s n�tida, con mejor iluminaci�n y sin inclinaci�n.";
                    }
                    else
                    {
                        ViewData["Mensaje"] = "Error al subir el archivo: " + errorContent;
                    }
                }
            }
            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Perfil()
        {
            var usuarioViewModel = ObtenerUsuarioDesdeClaims();
            if (usuarioViewModel == null)
            {
                return Unauthorized();
            }
            // Establecer el mensaje desde TempData a ViewData
            ViewData["Mensaje"] = TempData["Mensaje"];
            ViewData["TipoMensaje"] = TempData["TipoMensaje"];
            return View(usuarioViewModel);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login","Acceso");
        }

    }
}
