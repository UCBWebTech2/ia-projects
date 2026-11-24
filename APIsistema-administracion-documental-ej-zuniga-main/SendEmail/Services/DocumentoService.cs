//using Firebase.Auth;
//using Firebase.Storage;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using SendEmail.Context;
using SendEmail.DTOs;
using SendEmail.Models;
using Microsoft.EntityFrameworkCore;
//using TikaOnDotNet.TextExtraction;
using APIsEstudioJuridico.Models;
using APIsEstudioJuridico.DTOs;
using APIsEstudioJuridico.Services.Contrato;
using System.Text.Json;
using System.Text.RegularExpressions;
using SendEmail.Custom;
namespace APIsEstudioJuridico.Services
{
    public class DocumentoService
    {
        private readonly string _rutaServidor;
        private readonly string _cadenaSql;
        //private readonly FirebaseAuthClient _firebaseAuthClient;
        private readonly AppDbContext _context;
        private readonly IOcrApiClient _ocrApiClient;
        private readonly ISupabaseStorageService _supabaseStorageService;

        //public DocumentoService(AppDbContext context, IConfiguration config, FirebaseAuthClient firebaseAuthClient, HttpClient httpClient,ISupabaseStorageService supabaseStorageService)
        public DocumentoService(AppDbContext context, IConfiguration config, IOcrApiClient ocrApiClient, ISupabaseStorageService supabaseStorageService)
        {
            _context = context;
            //_rutaServidor = config.GetSection("Configuracion").GetSection("RutaServidor").Value;
            var rutaServidor = config.GetSection("Configuracion").GetSection("RutaServidor").Value;

            if (string.IsNullOrWhiteSpace(rutaServidor))
            {
                throw new ArgumentException("La configuración Configuracion:RutaServidor no está definida.");
            }
            _rutaServidor = rutaServidor;
            //_cadenaSql = config.GetConnectionString("CadenaSQL");
            var cadenaSql = config.GetConnectionString("CadenaSQL");

            if (string.IsNullOrWhiteSpace(cadenaSql))
            {
                throw new ArgumentException("La cadena de conexión CadenaSQL no está definida.");
            }

            _cadenaSql = cadenaSql;
            //_firebaseAuthClient = firebaseAuthClient;
            _ocrApiClient = ocrApiClient;
            _supabaseStorageService = supabaseStorageService;
        }
        private string CrearNombreSeguro(string nombreArchivoOriginal)
        {
            var extension = Path.GetExtension(nombreArchivoOriginal).ToLowerInvariant();
            var nombreSinExtension = Path.GetFileNameWithoutExtension(nombreArchivoOriginal);

            var nombreLimpio = Regex.Replace(nombreSinExtension, @"[^a-zA-Z0-9_\-\.]+", "_")
                                    .Trim('_');

            if (string.IsNullOrWhiteSpace(nombreLimpio))
            {
                nombreLimpio = "documento";
            }

            var fecha = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var identificador = Guid.NewGuid().ToString("N")[..8];

            return $"{fecha}_{identificador}_{nombreLimpio}{extension}";
        }
        private string CrearRutaVersionada(int idDoc, string nombreArchivoOriginal)
        {
            var extension = Path.GetExtension(nombreArchivoOriginal).ToLowerInvariant();
            var nombreSinExtension = Path.GetFileNameWithoutExtension(nombreArchivoOriginal);

            var nombreLimpio = Regex.Replace(nombreSinExtension, @"[^a-zA-Z0-9_\-\.]+", "_")
                                    .Trim('_');

            if (string.IsNullOrWhiteSpace(nombreLimpio))
            {
                nombreLimpio = "documento";
            }

            var fecha = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var identificador = Guid.NewGuid().ToString("N")[..8];

            return $"Documentos_Proyecto/{idDoc}/versiones/{fecha}_{identificador}_{nombreLimpio}{extension}";
        }
        private async Task<string> SubirVersionSupabaseAsync(IFormFile archivo, int idDoc)
        {
            var rutaVersion = CrearRutaVersionada(idDoc, archivo.FileName);

            var contentType = ObtenerContentTypePorExtension(
                archivo.FileName,
                archivo.ContentType
            );

            using var streamSupabase = archivo.OpenReadStream();

            var rutaGuardada = await _supabaseStorageService.UploadAsync(
                streamSupabase,
                rutaVersion,
                contentType
            );

            return rutaGuardada;
        }
        //private async Task<string> SubirStorage(Stream archivo, string nombre)
        //{
        //    //credenciales para subir archivos
        //    string email = "EJZuniga@gmail.com";
        //    string clave = "EjZuniga7099";
        //    string ruta = "proyectoejz.appspot.com";

        //    try
        //    {
        //        var auth = await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(email, clave);
        //        var firebaseToken = await auth.User.GetIdTokenAsync();

        //        var cancellation = new CancellationTokenSource();

        //        var task = new FirebaseStorage(
        //            ruta,
        //            new FirebaseStorageOptions
        //            {
        //                AuthTokenAsyncFactory = () => Task.FromResult(firebaseToken),
        //                ThrowOnCancel = true
        //            })
        //            .Child("Documentos_Proyecto")
        //            .Child(nombre)
        //            .PutAsync(archivo, cancellation.Token);


        //        var downloadURL = await task;


        //        return downloadURL;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error al subir el archivo: {ex.Message}");
        //    }
        //}
        //private async Task<string> SubirStorageSupabase(Stream archivo, string nombre)
        //{
        //    try
        //    {
        //        string carpeta = "Documentos_Proyecto";
        //        string rutaArchivo = $"{carpeta}/{nombre}";
        //        var rutaGuardada = await _supabaseStorageService.UploadAsync(
        //            archivo,
        //            rutaArchivo,
        //            "application/octet-stream"
        //        );
        //        return rutaGuardada;
        //    }
        //    catch(Exception ex)
        //    {
        //        throw new Exception($"Error al subir el archivo a Supabase: {ex.Message}");
        //    }
        //}
        private async Task<string> SubirStorageSupabase(Stream archivo, string nombre, string? contentTypeOriginal = null)
        {
            try
            {
                string carpeta = "Documentos_Proyecto";
                string nombreSeguro = CrearNombreSeguro(nombre);
                string rutaArchivo = $"{carpeta}/{nombreSeguro}";

                string contentType = ObtenerContentTypePorExtension(nombre, contentTypeOriginal);

                var rutaGuardada = await _supabaseStorageService.UploadAsync(
                    archivo,
                    rutaArchivo,
                    contentType
                );

                return rutaGuardada;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al subir el archivo a Supabase: {ex.Message}");
            }
        }
        public async Task<int> SubirDocumento(DocumentoDTO objeto, List<IndiceRespuestaDTO> entidades, ClaimsPrincipal user)
        {
            var idUsuario = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario))
                throw new UnauthorizedAccessException("No autorizado");
            if (objeto.Archivo == null || objeto.Archivo.Length == 0)
                throw new Exception("Debe enviar un archivo válido.");

            string nombreArchivo = objeto.Archivo.FileName;
            string rutaDocumento;
            using (var stream = objeto.Archivo.OpenReadStream())
            {
                //urlArchivo = await SubirStorage(stream, nombreArchivo);
                rutaDocumento = await SubirStorageSupabase(
                    stream,
                    nombreArchivo,
                    objeto.Archivo.ContentType
                );
            }

            string contenidoDocumento = objeto.ContenidoDocumento ?? string.Empty;

            using (var conexion = new SqlConnection(_cadenaSql))
            {
                conexion.Open();
                using (var transaction = conexion.BeginTransaction())
                {
                    try
                    {
                        var cmdDocumento = new SqlCommand("sp_guardar_documento", conexion, transaction);
                        cmdDocumento.CommandType = CommandType.StoredProcedure;
                        cmdDocumento.Parameters.AddWithValue("nombreDocumento", nombreArchivo);
                        cmdDocumento.Parameters.AddWithValue("rutaDocumento", rutaDocumento);
                        cmdDocumento.Parameters.AddWithValue("tipoDocumento", objeto.TipoDocumento);
                        cmdDocumento.Parameters.AddWithValue("idUsuario", int.Parse(idUsuario));
                        cmdDocumento.Parameters.AddWithValue("idCategoria", objeto.IdCategoria);
                        cmdDocumento.Parameters.AddWithValue("idProceso", objeto.IdProceso ?? (object)DBNull.Value);
                        cmdDocumento.Parameters.AddWithValue("habilitado", objeto.Habilitado);
                        cmdDocumento.Parameters.AddWithValue("contenidoDocumento", contenidoDocumento);

                        var idDocParam = new SqlParameter("idDoc", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        cmdDocumento.Parameters.Add(idDocParam);
                        cmdDocumento.ExecuteNonQuery();
                        int idDoc = (int)idDocParam.Value;

                        foreach (var entidad in entidades)
                        {
                            var cmdIndice = new SqlCommand("sp_guardar_indice", conexion, transaction);
                            cmdIndice.CommandType = CommandType.StoredProcedure;
                            cmdIndice.Parameters.AddWithValue("idDoc", idDoc);
                            cmdIndice.Parameters.AddWithValue("termino", entidad.Termino);
                            cmdIndice.Parameters.AddWithValue("tipoTermino", entidad.TipoTermino);
                            cmdIndice.Parameters.AddWithValue("frecuencia", entidad.Frecuencia);
                            cmdIndice.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        await GuardarEntidadesDocumentoAsync(idDoc, entidades);
                        return idDoc;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public async Task<string> ActualizarDocumentoConArchivo(int id, DocumentoDTO documentoDto, ClaimsPrincipal user)
        {
            var idUsuario = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario))
                throw new UnauthorizedAccessException("No autorizado");

            // Buscar documento existente
            var documentoExistente = await _context.Documentos.FindAsync(id);
            if (documentoExistente == null)
                throw new KeyNotFoundException("Documento no encontrado");

            //string rutaDocumento = documentoExistente.RutaDocumento;
            //string contenidoDocumento = documentoDto.ContenidoDocumento ?? documentoExistente.ContenidoDocumento;
            //**Versión mejorada para evitar problemas con nulls y mantener el contenido existente si no se proporciona uno nuevo.
            string rutaDocumento = documentoExistente.RutaDocumento ?? string.Empty;
            string contenidoDocumento = documentoDto.ContenidoDocumento
                ?? documentoExistente.ContenidoDocumento
                ?? string.Empty;

            List<IndiceRespuestaDTO> entidades = new();

            // Si hay un nuevo archivo, procesarlo
            var archivo = documentoDto.Archivo;
            string? rutaVersionSubida = null;
            string? contentTypeVersion = null;

            if (archivo != null && archivo.Length > 0)
            {
                contentTypeVersion = ObtenerContentTypePorExtension(
    archivo.FileName,
    archivo.ContentType
);

                // 1. Primero ejecutar OCR.
                // Si falla el OCR, no se sube el archivo a Supabase.
                using (var streamOCR = archivo.OpenReadStream())
                using (var content = new MultipartFormDataContent())
                {
                    var fileContent = new StreamContent(streamOCR);

                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(archivo.ContentType);

                    content.Add(fileContent, "file", archivo.FileName);

                    var response = await _ocrApiClient.PostAsync(
                        "SubirArchivo",
                        content,
                        user,
                        OcrPermissions.Process,
                        OcrOperations.SubirArchivo
                    );

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();

                        if ((int)response.StatusCode == StatusCodes.Status422UnprocessableEntity)
                        {
                            throw new InvalidOperationException(
                                "No se pudo extraer texto del documento porque la calidad de la imagen es muy baja. Intente subir una imagen más nítida, con mejor iluminación y sin inclinación."
                            );
                        }

                        throw new Exception($"Error al procesar el archivo: {response.ReasonPhrase}. Detalle: {errorContent}");
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(result);

                    if (!json.RootElement.TryGetProperty("extracted_text", out var extractedText) ||
                        !json.RootElement.TryGetProperty("entities", out var entitiesJson))
                    {
                        throw new Exception("No se pudo extraer texto o entidades del archivo.");
                    }

                    contenidoDocumento = extractedText.GetString() ?? string.Empty;

                    var entidadesDeserializadas =
                        JsonSerializer.Deserialize<List<IndiceRespuestaDTO>>(
                            entitiesJson.GetRawText()
                        );

                    entidades = entidadesDeserializadas ?? new List<IndiceRespuestaDTO>();
                }

                // 2. Si llegamos aquí, el OCR fue exitoso.
                // Ahora sí subimos la versión a Supabase.
                rutaVersionSubida = await SubirVersionSupabaseAsync(archivo, id);

                rutaDocumento = rutaVersionSubida;
            }
            //Versión Anterior
            //if (documentoDto.Archivo != null && documentoDto.Archivo.Length > 0)
            //{
            //    var archivo = documentoDto.Archivo;

            //    if (archivo != null && archivo.Length > 0)
            //    {
            //        using (var streamSupabase = archivo.OpenReadStream())
            //        {
            //            rutaDocumento = await SubirStorageSupabase(
            //                streamSupabase,
            //                archivo.FileName,
            //                archivo.ContentType
            //            );
            //        }

            //        using (var streamOCR = archivo.OpenReadStream())
            //        using (var content = new MultipartFormDataContent())
            //        {
            //            var fileContent = new StreamContent(streamOCR);

            //            fileContent.Headers.ContentType =
            //                new System.Net.Http.Headers.MediaTypeHeaderValue(archivo.ContentType);

            //            content.Add(fileContent, "file", archivo.FileName);

            //            var response = await _httpClient.PostAsync(
            //                $"{_bePythonUrl}/SubirArchivo",
            //                content
            //            );

            //            if (!response.IsSuccessStatusCode)
            //            {
            //                throw new Exception($"Error al procesar el archivo: {response.ReasonPhrase}");
            //            }

            //            var result = await response.Content.ReadAsStringAsync();
            //            var json = JsonDocument.Parse(result);

            //            if (!json.RootElement.TryGetProperty("extracted_text", out var extractedText) ||
            //                !json.RootElement.TryGetProperty("entities", out var entitiesJson))
            //            {
            //                throw new Exception("No se pudo extraer texto o entidades del archivo.");
            //            }

            //            contenidoDocumento = extractedText.GetString() ?? string.Empty;

            //            var entidadesDeserializadas =
            //                JsonSerializer.Deserialize<List<IndiceRespuestaDTO>>(
            //                    entitiesJson.GetRawText()
            //                );

            //            entidades = entidadesDeserializadas ?? new List<IndiceRespuestaDTO>();
            //        }
            //    }
            //}
            /* Versión remplazarArchivoEnMismaRuta
            var archivo = documentoDto.Archivo;

            if (archivo != null && archivo.Length > 0)
            {
                if (string.IsNullOrWhiteSpace(rutaDocumento))
                {
                    throw new Exception("El documento no tiene ruta asociada.");
                }

                if (rutaDocumento.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Este documento tiene una ruta antigua. Debe guardarse solo la ruta interna de Supabase.");
                }

                var extensionRutaActual = Path.GetExtension(rutaDocumento).ToLowerInvariant();
                var extensionArchivoNuevo = Path.GetExtension(archivo.FileName).ToLowerInvariant();

                if (extensionRutaActual != extensionArchivoNuevo)
                {
                    throw new Exception("Para reemplazar en la misma ruta, el archivo nuevo debe tener la misma extensión que el archivo actual.");
                }

                var contentType = ObtenerContentTypePorExtension(rutaDocumento, archivo.ContentType);

                using (var streamSupabase = archivo.OpenReadStream())
                {
                    rutaDocumento = await _supabaseStorageService.ReplaceAsync(
                        streamSupabase,
                        rutaDocumento,
                        contentType
                    );
                }

                using (var streamOCR = archivo.OpenReadStream())
                using (var content = new MultipartFormDataContent())
                {
                    var fileContent = new StreamContent(streamOCR);

                    fileContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(archivo.ContentType);

                    content.Add(fileContent, "file", archivo.FileName);

                    var response = await _ocrApiClient.PostAsync(
                        "SubirArchivo",
                        content,
                        user,
                        OcrPermissions.Process,
                        OcrOperations.SubirArchivo
                    );

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error al procesar el archivo: {response.ReasonPhrase}");
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(result);

                    if (!json.RootElement.TryGetProperty("extracted_text", out var extractedText) ||
                        !json.RootElement.TryGetProperty("entities", out var entitiesJson))
                    {
                        throw new Exception("No se pudo extraer texto o entidades del archivo.");
                    }

                    contenidoDocumento = extractedText.GetString() ?? string.Empty;

                    var entidadesDeserializadas =
                        JsonSerializer.Deserialize<List<IndiceRespuestaDTO>>(
                            entitiesJson.GetRawText()
                        );

                    entidades = entidadesDeserializadas ?? new List<IndiceRespuestaDTO>();
                }
            }*/
            if (string.IsNullOrWhiteSpace(rutaDocumento))
            {
                throw new Exception("El documento no tiene ruta asociada.");
            }
            using (var con = new SqlConnection(_cadenaSql))
            {
                con.Open();
                using (var transaction = con.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SqlCommand("spActualizarDocumento", con, transaction)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        cmd.Parameters.AddWithValue("idDoc", id);
                        cmd.Parameters.AddWithValue("nombreDocumento", documentoDto.NombreDocumento ?? documentoExistente.NombreDocumento);
                        cmd.Parameters.AddWithValue("rutaDocumento", rutaDocumento);
                        cmd.Parameters.AddWithValue("tipoDocumento", documentoDto.TipoDocumento ?? documentoExistente.TipoDocumento);
                        cmd.Parameters.AddWithValue("idUsuario", int.Parse(idUsuario));
                        cmd.Parameters.AddWithValue("idCategoria", documentoDto.IdCategoria);
                        cmd.Parameters.AddWithValue("idProceso", documentoDto.IdProceso ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("habilitado", documentoDto.Habilitado);
                        cmd.Parameters.AddWithValue("contenidoDocumento", contenidoDocumento);

                        await cmd.ExecuteNonQueryAsync();

                        if (entidades.Any())
                        {
                            foreach (var entidad in entidades)
                            {
                                var cmdIndice = new SqlCommand("sp_guardar_indice", con, transaction)
                                {
                                    CommandType = CommandType.StoredProcedure
                                };
                                cmdIndice.Parameters.AddWithValue("idDoc", id);
                                cmdIndice.Parameters.AddWithValue("termino", entidad.Termino);
                                cmdIndice.Parameters.AddWithValue("tipoTermino", entidad.TipoTermino);
                                cmdIndice.Parameters.AddWithValue("frecuencia", entidad.Frecuencia);
                                await cmdIndice.ExecuteNonQueryAsync();
                            }
                        }

                        transaction.Commit();
                        if (entidades.Any())
                            await GuardarEntidadesDocumentoAsync(id, entidades);
                        return "Documento actualizado exitosamente";
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        //public async Task<string> ProcesarYSubirDocumento(DocumentoDTO objeto, ClaimsPrincipal user)
        //{
        //    var idUsuario = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(idUsuario))
        //        throw new UnauthorizedAccessException("No autorizado");
        //    if (objeto.Archivo == null || objeto.Archivo.Length == 0)
        //    {
        //        throw new Exception("Debe enviar un archivo válido.");
        //    }
        //    var archivo = objeto.Archivo;
        //    // Paso 1: Extraer el texto del archivo
        //    string contenidoDocumento = string.Empty;
        //    string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName));

        //    // Guardar el archivo en el disco temporalmente
        //    using (var stream = archivo.OpenReadStream())
        //    {
        //        using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
        //        {
        //            await stream.CopyToAsync(fileStream);
        //        }
        //    }

        //    var extractor = new TextExtractor();
        //    var result = extractor.Extract(tempFilePath);
        //    contenidoDocumento = result.Text;

        //    File.Delete(tempFilePath);
        //    //string urlArchivo = await SubirStorage(objeto.Archivo.OpenReadStream(), nombreArchivo);
        //    string nombreArchivo = archivo.FileName;
        //    string rutaDocumento;
        //    using (var streamArchivo = archivo.OpenReadStream())
        //    {
        //        rutaDocumento = await SubirStorageSupabase(
        //                streamArchivo,
        //                nombreArchivo,
        //                archivo.ContentType
        //        );
        //    }

        //    using (var conexion = new SqlConnection(_cadenaSql))
        //    {
        //        conexion.Open();
        //        using (var transaction = conexion.BeginTransaction())
        //        {
        //            try
        //            {
        //                var cmd = new SqlCommand("sp_guardar_documento", conexion, transaction);
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                cmd.Parameters.AddWithValue("nombreDocumento", nombreArchivo);
        //                cmd.Parameters.AddWithValue("rutaDocumento", rutaDocumento);
        //                cmd.Parameters.AddWithValue("tipoDocumento", objeto.TipoDocumento);
        //                cmd.Parameters.AddWithValue("idUsuario", int.Parse(idUsuario));
        //                cmd.Parameters.AddWithValue("idCategoria", objeto.IdCategoria);
        //                cmd.Parameters.AddWithValue("idProceso", objeto.IdProceso ?? (object)DBNull.Value);
        //                cmd.Parameters.AddWithValue("habilitado", objeto.Habilitado);
        //                cmd.Parameters.AddWithValue("contenidoDocumento", contenidoDocumento);

        //                var idDocParam = new SqlParameter("idDoc", SqlDbType.Int) { Direction = ParameterDirection.Output };
        //                cmd.Parameters.Add(idDocParam);

        //                cmd.ExecuteNonQuery();

        //                int idDoc = (int)idDocParam.Value;

        //                transaction.Commit();

        //                return rutaDocumento;
        //            }
        //            catch (Exception)
        //            {
        //                transaction.Rollback();
        //                throw;
        //            }
        //        }
        //    }
        //}

        public async Task<List<DocumentoDTO>> ListaDocumentos()
        {
            var listaDTO = new List<DocumentoDTO>();
            var listaDB = await _context.Documentos
                .Include(c => c.CategoriaReferencia)
                .Include(u=>u.IdUsuarioNavigation)
                .Include(p => p.ProcesoReferencia)
                .ToListAsync();
            foreach(var item in listaDB)
            {
                listaDTO.Add(new DocumentoDTO
                {
                    IdDoc = item.IdDoc,
                    NombreDocumento = item.NombreDocumento ?? "Sin nombre",
                    RutaDocumento = item.RutaDocumento ?? "Sin ruta",
                    TipoDocumento = item.TipoDocumento ?? "Desconocido",
                    //Estado = item.Estado ?? "Estado no especificado",
                    Habilitado=item.Habilitado,
                    IdUsuario = item.IdUsuario,
                    IdCategoria = item.IdCategoria,
                    IdProceso=item.IdProceso,
                    NombreUsuario = item.IdUsuarioNavigation.NombreUsuario,
                    NombreCategoria = item.CategoriaReferencia.NombreCategoria,
                    NombreProceso = item.ProcesoReferencia?.NombreProceso ?? "Sin proceso",
                    FechaCreacion = item.FechaCreacion
                });
            }
            return listaDTO;
        }//ObtenerDocumentoPorIdAsync
        public async Task<DocumentoDTO> ObtenerDocumentoPorId(int id)
        {
            var documentoDB = await _context.Documentos
                .Include(d => d.CategoriaReferencia)
                .FirstOrDefaultAsync(d => d.IdDoc == id);

            if (documentoDB == null)
                throw new KeyNotFoundException("Documento no encontrado");
            return new DocumentoDTO
            {
                IdDoc = documentoDB.IdDoc,
                NombreDocumento = documentoDB.NombreDocumento,
                RutaDocumento = documentoDB.RutaDocumento,
                TipoDocumento = documentoDB.TipoDocumento,
                //Estado = documentoDB.Estado,
                IdUsuario = documentoDB.IdUsuario,
                NombreUsuario = documentoDB.IdUsuarioNavigation?.NombreUsuario,
                IdCategoria = documentoDB.IdCategoria,
                NombreCategoria = documentoDB.CategoriaReferencia?.NombreCategoria
            };
        }
        //ObtenerDocumentoPorIdAsync
        public async Task<object> ObtenerDocumentoPorIdAsync(int id)
        {
            try
            {
                var documento = await _context.Documentos
                    .Where(d => d.IdDoc == id)
                    .Select(d => new
                    {
                        d.IdDoc,
                        d.NombreDocumento,
                        d.RutaDocumento,
                        d.TipoDocumento,
                        d.IdUsuario
                    })
                    .FirstOrDefaultAsync();
                if (documento == null)
                {
                    throw new KeyNotFoundException("Documento no encontrado");
                }
                return documento;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<DocumentoDTO>> ObtenerDocumentosPorProceso(int idProceso)
        {
            var listaDocumentos = new List<DocumentoDTO>();

            using (var conexion = new SqlConnection(_cadenaSql))
            {
                await conexion.OpenAsync();

                using (var cmd = new SqlCommand("sp_ObtenerDocumentosPorProceso", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parámetro para el ID del proceso
                    cmd.Parameters.AddWithValue("@idProceso", idProceso);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            listaDocumentos.Add(new DocumentoDTO
                            {
                                IdDoc = reader.GetInt32(0),
                                NombreDocumento = reader.IsDBNull(1) ? "Sin nombre" : reader.GetString(1),
                                RutaDocumento = reader.IsDBNull(2) ? "Sin ruta" : reader.GetString(2),
                                TipoDocumento = reader.IsDBNull(3) ? "Desconocido" : reader.GetString(3),
                                IdUsuario = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                NombreUsuario = reader.IsDBNull(5) ? "Usuario no especificado" : reader.GetString(5),
                                IdCategoria = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                NombreCategoria = reader.IsDBNull(7) ? "Categoría no especificada" : reader.GetString(7),
                                IdProceso = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8),
                                Habilitado = !reader.IsDBNull(9) && reader.GetBoolean(9),
                                FechaCreacion = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                                NombreProceso = "Proceso no especificado"
                            });

                        }
                    }
                }
            }

            return listaDocumentos;
        }
        public async Task<DocumentoDetalleDTO?> ObtenerDetallesDocumentoAsync(int id)
        {
            // Obtener el documento principal
            var documentoDB = await _context.Documentos
                .Include(u => u.IdUsuarioNavigation)
                .Include(c => c.CategoriaReferencia)
                .Include(p => p.ProcesoReferencia)
                .FirstOrDefaultAsync(d => d.IdDoc == id);

            if (documentoDB == null)
            {
                return null;
            }

            var tiposPermitidos = new[] { "ARTICULO","LEY","INSTRUMENTO","INSTITUCION","ORG","PER","LOC","DEMANDANTE","DEMANDADO","JUEZ","FISCAL","ABOGADO","NRO_PROCESO","JUZGADO","FECHA_AUDIENCIA" };
            var indicesRelacionados = await _context.Indices
            .Where(i => i.IdDoc == id && i.TipoTermino != null && tiposPermitidos.Contains(i.TipoTermino))
            .ToListAsync();

            List<IndiceDTO> entidades = new();
            try
            {
                entidades = await _context.DocumentoEntidades.AsNoTracking()
                    .Where(e => e.IdDoc == id)
                    .OrderBy(e => e.TipoEntidad).ThenByDescending(e => e.Confianza)
                    .Select(e => new IndiceDTO { Termino = e.Valor, TipoTermino = e.TipoEntidad, Frecuencia = 1, IdDoc = e.IdDoc })
                    .ToListAsync();
            }
            catch { /* tabla puede no existir */ }

            // Construir el DTO
            var documentoDetalleDTO = new DocumentoDetalleDTO
            {
                IdDocumento = documentoDB.IdDoc,
                NombreDocumento = documentoDB.NombreDocumento ?? "Sin nombre",
                TipoDocumento = documentoDB.TipoDocumento ?? "Desconocido",
                NombreUsuario = documentoDB.IdUsuarioNavigation?.NombreUsuario ?? "Sin usuario",
                FechaSubida = documentoDB.FechaCreacion ?? DateTime.MinValue,
                Habilitado = documentoDB.Habilitado,
                PrediccionIa = documentoDB.PrediccionIa,
                ConfianzaIa = documentoDB.ConfianzaIa,
                VersionModelo = documentoDB.VersionModelo,
                CategoriaConfirmadaId = documentoDB.CategoriaConfirmadaId,
                Indices = indicesRelacionados.Select(i => new IndiceDTO
                {
                    Termino = i.Termino,
                    TipoTermino = i.TipoTermino,
                    Frecuencia = i.Frecuencia,
                    IdDoc = i.IdDoc
                }).ToList(),
                Entidades = entidades.Count > 0 ? entidades : indicesRelacionados.Select(i => new IndiceDTO { Termino = i.Termino, TipoTermino = i.TipoTermino, Frecuencia = i.Frecuencia, IdDoc = i.IdDoc }).ToList()
            };

            return documentoDetalleDTO;
        }

        public async Task<bool> CambiarEstado(int id)
        {
            // Obtener el documento principal
            var documentoDB = await _context.Documentos.AsNoTracking().FirstOrDefaultAsync(d => d.IdDoc == id);

            if (documentoDB == null)
            {
                return false;
            }
            documentoDB.Habilitado = !documentoDB.Habilitado;
            try
            {
                _context.Documentos.Update(documentoDB);
                await _context.SaveChangesAsync();
                return true; // Indica éxito
            }
            catch (DbUpdateException ex)
            {
                // Log o manejo de error específico de la base de datos
                throw new Exception("Error al actualizar el estado en la base de datos", ex);
            }
            catch (Exception ex)
            {
                // Captura errores inesperados
                throw new Exception("Error inesperado al actualizar el estado en la en la base de datos", ex);
            }
        }

        private static string NormalizarValorEntidad(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return string.Empty;
            var normalizado = valor.Trim().ToUpperInvariant();
            normalizado = Regex.Replace(normalizado, @"\s+", " ");
            return normalizado;
        }

        private async Task RegistrarBusquedaAsync(int? idUsuario, string tipoBusqueda, string? query, object? filtros, int resultados, long tiempoMs)
        {
            try
            {
                _context.Busquedas.Add(new Busqueda
                {
                    IdUsuario = idUsuario,
                    TipoBusqueda = tipoBusqueda,
                    Query = query,
                    FiltrosJson = filtros == null ? null : JsonSerializer.Serialize(filtros),
                    Resultados = resultados,
                    TiempoMs = (int)Math.Min(tiempoMs, int.MaxValue),
                    Fecha = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            catch
            {
                // No bloquear la búsqueda si falla el logging de métricas.
            }
        }

        private async Task GuardarEntidadesDocumentoAsync(int idDoc, IEnumerable<IndiceRespuestaDTO> entidades)
        {
            var lista = (entidades ?? Enumerable.Empty<IndiceRespuestaDTO>())
                .Where(e => !string.IsNullOrWhiteSpace(e.Termino) && !string.IsNullOrWhiteSpace(e.TipoTermino))
                .Select(e => new DocumentoEntidad
                {
                    IdDoc = idDoc,
                    TipoEntidad = e.TipoTermino!.Trim().ToUpperInvariant(),
                    Valor = e.Termino!.Trim(),
                    ValorNormalizado = NormalizarValorEntidad(e.Termino),
                    Fuente = "NER",
                    Confianza = null,
                    FechaCreacion = DateTime.UtcNow
                })
                .ToList();

            if (lista.Count == 0) return;

            try
            {
                var existentes = await _context.DocumentoEntidades.Where(x => x.IdDoc == idDoc).ToListAsync();
                if (existentes.Count > 0) _context.DocumentoEntidades.RemoveRange(existentes);
                await _context.DocumentoEntidades.AddRangeAsync(lista);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // No bloquear el guardado principal si falla la tabla de entidades (migración pendiente).
            }
        }

        public async Task<List<DocumentoDTO>> BuscarDocumentosConFiltros(FiltroBusqueda filtro, int? idUsuario = null)
        {
            var sw = Stopwatch.StartNew();
            var documentos = new List<DocumentoDTO>();
            bool extended = filtro.IdUsuario.HasValue || filtro.AbogadoResponsableId.HasValue || !string.IsNullOrWhiteSpace(filtro.ClienteNombre)
                || !string.IsNullOrWhiteSpace(filtro.EstadoDocumento) || !string.IsNullOrWhiteSpace(filtro.TipoEntidad) || !string.IsNullOrWhiteSpace(filtro.ValorEntidad);

            try
            {
                var q = _context.Documentos.AsNoTracking()
                    .Include(d => d.IdUsuarioNavigation)
                    .Include(d => d.CategoriaReferencia)
                    .Include(d => d.ProcesoReferencia)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(filtro.Query))
                {
                    var qtxt = filtro.Query.Trim();
                    q = q.Where(d => (d.NombreDocumento ?? "").Contains(qtxt) || (d.ContenidoDocumento ?? "").Contains(qtxt));
                }
                if (!string.IsNullOrWhiteSpace(filtro.TipoDocumento)) q = q.Where(d => d.TipoDocumento == filtro.TipoDocumento);
                if (filtro.FechaInicio.HasValue) q = q.Where(d => d.FechaCreacion >= filtro.FechaInicio.Value);
                if (filtro.FechaFin.HasValue) q = q.Where(d => d.FechaCreacion <= filtro.FechaFin.Value);
                if (filtro.Categoria.HasValue) q = q.Where(d => d.IdCategoria == filtro.Categoria.Value);
                if (filtro.Proceso.HasValue) q = q.Where(d => d.IdProceso == filtro.Proceso.Value);
                if (filtro.IdUsuario.HasValue) q = q.Where(d => d.IdUsuario == filtro.IdUsuario.Value);
                if (filtro.AbogadoResponsableId.HasValue) q = q.Where(d => d.AbogadoResponsableId == filtro.AbogadoResponsableId.Value);
                if (!string.IsNullOrWhiteSpace(filtro.ClienteNombre)) q = q.Where(d => (d.ClienteNombre ?? "").Contains(filtro.ClienteNombre));
                if (!string.IsNullOrWhiteSpace(filtro.EstadoDocumento)) q = q.Where(d => d.EstadoDocumento == filtro.EstadoDocumento);

                if (!string.IsNullOrWhiteSpace(filtro.TipoEntidad) || !string.IsNullOrWhiteSpace(filtro.ValorEntidad))
                {
                    var tipo = string.IsNullOrWhiteSpace(filtro.TipoEntidad) ? null : filtro.TipoEntidad.Trim().ToUpperInvariant();
                    var val = NormalizarValorEntidad(filtro.ValorEntidad);
                    var eq = _context.DocumentoEntidades.AsNoTracking().AsQueryable();
                    if (tipo != null) eq = eq.Where(e => e.TipoEntidad == tipo);
                    if (!string.IsNullOrWhiteSpace(val)) eq = eq.Where(e => (e.ValorNormalizado ?? "").Contains(val));
                    var ids = eq.Select(e => e.IdDoc);
                    q = q.Where(d => ids.Contains(d.IdDoc));
                }

                documentos = await q.OrderByDescending(d => d.FechaCreacion).Take(300)
                    .Select(d => new DocumentoDTO
                    {
                        IdDoc = d.IdDoc,
                        NombreDocumento = d.NombreDocumento ?? "Sin nombre",
                        RutaDocumento = d.RutaDocumento ?? "Sin ruta",
                        TipoDocumento = d.TipoDocumento ?? "Desconocido",
                        IdUsuario = d.IdUsuario,
                        NombreUsuario = d.IdUsuarioNavigation != null ? d.IdUsuarioNavigation.NombreUsuario ?? "Usuario no especificado" : "Usuario no especificado",
                        IdCategoria = d.IdCategoria,
                        NombreCategoria = d.CategoriaReferencia != null ? d.CategoriaReferencia.NombreCategoria ?? "Categoría no especificada" : "Categoría no especificada",
                        IdProceso = d.IdProceso,
                        NombreProceso = d.ProcesoReferencia != null ? d.ProcesoReferencia.NombreProceso ?? "Proceso no especificado" : "Proceso no especificado",
                        FechaCreacion = d.FechaCreacion,
                        Habilitado = d.Habilitado
                    }).ToListAsync();
            }
            catch
            {
                if (extended) throw;
                using var conexion = new SqlConnection(_cadenaSql);
                await conexion.OpenAsync();
                using var cmd = new SqlCommand("sp_BuscarDocumentos", conexion) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Query", string.IsNullOrWhiteSpace(filtro.Query) ? (object)DBNull.Value : filtro.Query);
                cmd.Parameters.AddWithValue("@TipoDocumento", (object?)filtro.TipoDocumento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaInicio", (object?)filtro.FechaInicio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaFin", (object?)filtro.FechaFin ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Categoria", (object?)filtro.Categoria ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Proceso", (object?)filtro.Proceso ?? DBNull.Value);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    documentos.Add(new DocumentoDTO
                    {
                        IdDoc = reader.GetInt32(0),
                        NombreDocumento = reader.IsDBNull(1) ? "Sin nombre" : reader.GetString(1),
                        RutaDocumento = reader.IsDBNull(2) ? "Sin ruta" : reader.GetString(2),
                        TipoDocumento = reader.IsDBNull(3) ? "Desconocido" : reader.GetString(3),
                        IdUsuario = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        NombreUsuario = reader.IsDBNull(5) ? "Usuario no especificado" : reader.GetString(5),
                        IdCategoria = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                        NombreCategoria = reader.IsDBNull(7) ? "Categoría no especificada" : reader.GetString(7),
                        IdProceso = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8),
                        NombreProceso = reader.IsDBNull(9) ? "Proceso no especificado" : reader.GetString(9),
                        FechaCreacion = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                        Habilitado = !reader.IsDBNull(11) && reader.GetBoolean(11)
                    });
                }
            }

            sw.Stop();
            await RegistrarBusquedaAsync(idUsuario, "FILTROS", filtro.Query, filtro, documentos.Count, sw.ElapsedMilliseconds);
            return documentos;
        }

        public async Task<List<DocumentoDTO>> BuscarDocumentosPorContenido(string query, int? idUsuario = null)
        {
            var sw = Stopwatch.StartNew();
            var documentos = new List<DocumentoDTO>();

            using (var conexion = new SqlConnection(_cadenaSql))
            {
                await conexion.OpenAsync();

                var sqlQuery = @"
            SELECT 
                d.IdDoc,
                d.NombreDocumento,
                d.RutaDocumento,
                d.TipoDocumento,
                d.IdUsuario,
                u.NombreUsuario,
                d.IdCategoria,
                c.NombreCategoria,
                d.IdProceso,
                p.NombreProceso,
                d.FechaCreacion
            FROM 
                Documentos d
            LEFT JOIN 
                Usuarios u ON d.IdUsuario = u.IdUsuario
            LEFT JOIN 
                Categorias c ON d.IdCategoria = c.IdCategoria
            LEFT JOIN 
                Procesos p ON d.IdProceso = p.IdProceso
            WHERE 
                CONTAINS(d.contenidoDocumento, @query)";

                using (var cmd = new SqlCommand(sqlQuery, conexion))
                {
                    cmd.Parameters.AddWithValue("@query", query);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            documentos.Add(new DocumentoDTO
                            {
                                IdDoc = reader.GetInt32(0),
                                NombreDocumento = reader.IsDBNull(1) ? "Sin nombre" : reader.GetString(1),
                                RutaDocumento = reader.IsDBNull(2) ? "Sin ruta" : reader.GetString(2),
                                TipoDocumento = reader.IsDBNull(3) ? "Desconocido" : reader.GetString(3),

                                IdUsuario = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                NombreUsuario = reader.IsDBNull(5) ? "Usuario no especificado" : reader.GetString(5),

                                IdCategoria = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                NombreCategoria = reader.IsDBNull(7) ? "Categoría no especificada" : reader.GetString(7),

                                IdProceso = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8),
                                NombreProceso = reader.IsDBNull(9) ? "Proceso no especificado" : reader.GetString(9),

                                FechaCreacion = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10)
                            });

                        }
                    }
                }
            }
            sw.Stop();
            await RegistrarBusquedaAsync(idUsuario, "CONTENIDO", query, new { query }, documentos.Count, sw.ElapsedMilliseconds);
            return documentos;
        }
        public async Task<List<DocumentoDTO>> BuscarDocumentosPorEntidad(string? tipoEntidad, string? valor, int? idUsuario = null)
        {
            var sw = Stopwatch.StartNew();
            var documentos = new List<DocumentoDTO>();
            var tipo = string.IsNullOrWhiteSpace(tipoEntidad) ? null : tipoEntidad.Trim().ToUpperInvariant();
            var valorNorm = NormalizarValorEntidad(valor);

            try
            {
                var q = _context.DocumentoEntidades.AsNoTracking().Where(e => e.IdDoc > 0);
                if (!string.IsNullOrWhiteSpace(tipo))
                    q = q.Where(e => e.TipoEntidad == tipo);
                if (!string.IsNullOrWhiteSpace(valorNorm))
                    q = q.Where(e => (e.ValorNormalizado ?? "").Contains(valorNorm));

                var ids = await q.Select(e => e.IdDoc).Distinct().Take(200).ToListAsync();
                documentos = await _context.Documentos.AsNoTracking()
                    .Where(d => ids.Contains(d.IdDoc))
                    .Include(d => d.IdUsuarioNavigation)
                    .Include(d => d.CategoriaReferencia)
                    .Include(d => d.ProcesoReferencia)
                    .OrderByDescending(d => d.FechaCreacion)
                    .Select(d => new DocumentoDTO
                    {
                        IdDoc = d.IdDoc,
                        NombreDocumento = d.NombreDocumento ?? "Sin nombre",
                        RutaDocumento = d.RutaDocumento ?? "Sin ruta",
                        TipoDocumento = d.TipoDocumento ?? "Desconocido",
                        IdUsuario = d.IdUsuario,
                        NombreUsuario = d.IdUsuarioNavigation != null ? d.IdUsuarioNavigation.NombreUsuario ?? "Usuario no especificado" : "Usuario no especificado",
                        IdCategoria = d.IdCategoria,
                        NombreCategoria = d.CategoriaReferencia != null ? d.CategoriaReferencia.NombreCategoria ?? "Categoría no especificada" : "Categoría no especificada",
                        IdProceso = d.IdProceso,
                        NombreProceso = d.ProcesoReferencia != null ? d.ProcesoReferencia.NombreProceso ?? "Proceso no especificado" : "Proceso no especificado",
                        FechaCreacion = d.FechaCreacion,
                        Habilitado = d.Habilitado
                    })
                    .ToListAsync();
            }
            catch
            {
                var qi = _context.Indices.AsNoTracking().Where(i => i.IdDoc != null);
                if (!string.IsNullOrWhiteSpace(tipo))
                    qi = qi.Where(i => i.TipoTermino == tipo);
                if (!string.IsNullOrWhiteSpace(valorNorm))
                    qi = qi.Where(i => (i.Termino ?? "").ToUpper().Contains(valorNorm));

                var ids = await qi.Select(i => i.IdDoc!.Value).Distinct().Take(200).ToListAsync();
                documentos = await _context.Documentos.AsNoTracking()
                    .Where(d => ids.Contains(d.IdDoc))
                    .Include(d => d.IdUsuarioNavigation)
                    .Include(d => d.CategoriaReferencia)
                    .Include(d => d.ProcesoReferencia)
                    .OrderByDescending(d => d.FechaCreacion)
                    .Select(d => new DocumentoDTO
                    {
                        IdDoc = d.IdDoc,
                        NombreDocumento = d.NombreDocumento ?? "Sin nombre",
                        RutaDocumento = d.RutaDocumento ?? "Sin ruta",
                        TipoDocumento = d.TipoDocumento ?? "Desconocido",
                        IdUsuario = d.IdUsuario,
                        NombreUsuario = d.IdUsuarioNavigation != null ? d.IdUsuarioNavigation.NombreUsuario ?? "Usuario no especificado" : "Usuario no especificado",
                        IdCategoria = d.IdCategoria,
                        NombreCategoria = d.CategoriaReferencia != null ? d.CategoriaReferencia.NombreCategoria ?? "Categoría no especificada" : "Categoría no especificada",
                        IdProceso = d.IdProceso,
                        NombreProceso = d.ProcesoReferencia != null ? d.ProcesoReferencia.NombreProceso ?? "Proceso no especificado" : "Proceso no especificado",
                        FechaCreacion = d.FechaCreacion,
                        Habilitado = d.Habilitado
                    })
                    .ToListAsync();
            }

            sw.Stop();
            await RegistrarBusquedaAsync(idUsuario, "ENTIDAD", valor, new { tipoEntidad = tipo, valor }, documentos.Count, sw.ElapsedMilliseconds);
            return documentos;
        }

        public async Task<string> ObtenerUrlFirmadaDocumento(int id, ClaimsPrincipal user)
        {
            var idUsuario = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuario))
                throw new UnauthorizedAccessException("No autorizado");

            var documento = await _context.Documentos.FindAsync(id);

            if (documento == null)
                throw new KeyNotFoundException("Documento no encontrado");

            var rutaDocumento = documento.RutaDocumento;

            if (string.IsNullOrWhiteSpace(rutaDocumento))
                throw new Exception("El documento no tiene ruta asociada.");

            // Esto es para evitar usar registros antiguos que quedaron como URL pública completa.
            if (rutaDocumento.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Este documento tiene una ruta antigua. Debe guardarse solo la ruta interna de Supabase.");

            // 3600 segundos = 1 hora
            var urlFirmada = await _supabaseStorageService.CreateSignedUrlAsync(rutaDocumento, 3600);

            return urlFirmada;
        }
        //private string ObtenerContentTypePorExtension(string nombreArchivo, string? contentTypeOriginal = null)
        //{
        //    var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();

        //    return extension switch
        //    {
        //        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        //        ".doc" => "application/msword",
        //        ".pdf" => "application/pdf",
        //        ".txt" => "text/plain",
        //        ".jpg" or ".jpeg" => "image/jpeg",
        //        ".png" => "image/png",

        //        _ => !string.IsNullOrWhiteSpace(contentTypeOriginal)
        //            ? contentTypeOriginal
        //            : "application/octet-stream"
        //    };
        //}
        private string ObtenerContentTypePorExtension(string nombreArchivo, string? contentTypeOriginal = null)
        {
            var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();

            return extension switch
            {
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",

                _ => !string.IsNullOrWhiteSpace(contentTypeOriginal)
                    ? contentTypeOriginal
                    : "application/octet-stream"
            };
        }
        public async Task<string> ReemplazarArchivoEnMismaRuta(int id, IFormFile archivo, ClaimsPrincipal user)
        {
            var idUsuario = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuario))
                throw new UnauthorizedAccessException("No autorizado");

            if (archivo == null || archivo.Length == 0)
                throw new Exception("Debe enviar un archivo válido.");

            var documento = await _context.Documentos.FindAsync(id);

            if (documento == null)
                throw new KeyNotFoundException("Documento no encontrado");

            var rutaDocumento = documento.RutaDocumento;

            if (string.IsNullOrWhiteSpace(rutaDocumento))
                throw new Exception("El documento no tiene ruta asociada.");

            if (rutaDocumento.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Este documento tiene una ruta antigua. Debe guardarse solo la ruta interna de Supabase.");
            var extensionRuta = Path.GetExtension(rutaDocumento).ToLowerInvariant();
            var extensiobArchivo = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (extensionRuta != extensiobArchivo)
            {
                throw new Exception("El archivo nuevo debe tener la misma extensión que el archivo actual.");
            }
            // Usamos la extensión de la ruta guardada en BD.
            // Ejemplo: Documentos_Proyecto/MODELO ACCION....docx
            var contentType = ObtenerContentTypePorExtension(rutaDocumento, archivo.ContentType);

            using var stream = archivo.OpenReadStream();

            var rutaGuardada = await _supabaseStorageService.ReplaceAsync(
                stream,
                rutaDocumento,
                contentType
            );

            return rutaGuardada;
        }

        public async Task<bool> ConfirmarPrediccionAsync(ConfirmarPrediccionDTO dto, ClaimsPrincipal user)
        {
            var idUsuario = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario))
                throw new UnauthorizedAccessException("No autorizado");

            var documento = await _context.Documentos.FindAsync(dto.IdDocumento);
            if (documento == null) throw new KeyNotFoundException("Documento no encontrado");

            var categoria = await _context.Categorias.FindAsync(dto.IdCategoriaConfirmada);
            if (categoria == null) throw new KeyNotFoundException("Categoría no encontrada");

            var anterior = documento.IdCategoria;
            documento.CategoriaConfirmadaId = dto.IdCategoriaConfirmada;
            documento.ConfirmadoPor = int.TryParse(idUsuario, out var uid) ? uid : null;
            documento.FechaConfirmacion = DateTime.UtcNow;
            documento.FechaModificacion = DateTime.Now;
            documento.IdCategoria = dto.IdCategoriaConfirmada;

            try
            {
                _context.ClasificacionHistoriales.Add(new ClasificacionHistorial
                {
                    IdDoc = documento.IdDoc,
                    CategoriaAnteriorId = anterior,
                    CategoriaNuevaId = dto.IdCategoriaConfirmada,
                    PrediccionIa = documento.PrediccionIa,
                    ConfianzaIa = documento.ConfianzaIa,
                    VersionModelo = documento.VersionModelo,
                    Origen = "USUARIO",
                    IdUsuario = documento.ConfirmadoPor,
                    Fecha = DateTime.UtcNow,
                    Comentario = dto.Comentario
                });
            }
            catch { /* tabla puede no existir aún */ }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<object>> ObtenerHistorialClasificacionAsync(int idDoc)
        {
            try
            {
                return await _context.ClasificacionHistoriales
                    .AsNoTracking()
                    .Where(h => h.IdDoc == idDoc)
                    .OrderByDescending(h => h.Fecha)
                    .Select(h => (object)new
                    {
                        h.IdHistorial,
                        h.IdDoc,
                        h.CategoriaAnteriorId,
                        h.CategoriaNuevaId,
                        h.PrediccionIa,
                        h.ConfianzaIa,
                        h.VersionModelo,
                        h.Origen,
                        h.IdUsuario,
                        h.Fecha,
                        h.Comentario
                    })
                    .ToListAsync();
            }
            catch
            {
                return new List<object>();
            }
        }
    }
}
