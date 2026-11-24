using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using APIsEstudioJuridico.Services.Contrato;
namespace APIsEstudioJuridico.Services.Implementacion
{
    public class SupabaseStorageService:ISupabaseStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _supabaseUrl;
        private readonly string _serviceKey;
        private readonly string _bucket;
        private readonly string _localPath;
        private readonly bool _isConfigured;

        public SupabaseStorageService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _supabaseUrl = (configuration["Supabase:Url"] ?? string.Empty).TrimEnd('/');
            _serviceKey = configuration["Supabase:ServiceKey"] ?? string.Empty;
            _bucket = configuration["Supabase:Bucket"] ?? "documentos";
            _localPath = configuration["Storage:LocalPath"] ?? "/app/DocumentosSistema";
            _isConfigured = !string.IsNullOrWhiteSpace(_supabaseUrl) && !string.IsNullOrWhiteSpace(_serviceKey);

            if (_isConfigured)
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _serviceKey);

                if (!_httpClient.DefaultRequestHeaders.Contains("apikey"))
                {
                    _httpClient.DefaultRequestHeaders.Add("apikey", _serviceKey);
                }
            }
            else
            {
                Directory.CreateDirectory(_localPath);
            }
        }

        public async Task<string> UploadAsync(Stream stream, string path, string contentType = "application/octet-stream")
        {
            if (!_isConfigured)
            {
                var fullPath = Path.Combine(_localPath, path.Replace('/', Path.DirectorySeparatorChar));
                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(fs);
                return path;
            }

            var encodedPath = EncodePath(path);
            var url = $"{_supabaseUrl}/storage/v1/object/{_bucket}/{encodedPath}";


            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var response = await _httpClient.PutAsync(url, content);

            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error Supabase Upload: {response.StatusCode} - {responseText}");
            }

            return path;
        }
        public bool IsConfigured => _isConfigured;
        public string LocalPath => _localPath;

        public Task<string> CreateSignedUrlAsync(string path, int expiresInSeconds = 3600)
        {
            if (!_isConfigured)
            {
                return Task.FromResult($"LOCAL:{path}");
            }

            return CreateSignedUrlRemoteAsync(path, expiresInSeconds);
        }

        private async Task<string> CreateSignedUrlRemoteAsync(string path, int expiresInSeconds)
        {
            var encodedPath = EncodePath(path);

            var url = $"{_supabaseUrl}/storage/v1/object/sign/{_bucket}/{encodedPath}";

            var body = new
            {
                expiresIn = expiresInSeconds
            };

            var json = JsonSerializer.Serialize(body);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error Supabase Signed URL: {response.StatusCode} - {responseText}");
            }

            using var document = JsonDocument.Parse(responseText);

            string? signedUrl = null;

            if (document.RootElement.TryGetProperty("signedURL", out var signedUrlUpper))
            {
                signedUrl = signedUrlUpper.GetString();
            }
            else if (document.RootElement.TryGetProperty("signedUrl", out var signedUrlLower))
            {
                signedUrl = signedUrlLower.GetString();
            }

            if (string.IsNullOrWhiteSpace(signedUrl))
            {
                throw new Exception($"Supabase no devolvió signedURL. Respuesta: {responseText}");
            }

            if (signedUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return signedUrl;
            }

            return $"{_supabaseUrl}/storage/v1{signedUrl}";
        }
        public async Task<string> ReplaceAsync(Stream stream, string path, string contentType = "application/octet-stream")
        {
            if (!_isConfigured)
            {
                var fullPath = Path.Combine(_localPath, path.Replace('/', Path.DirectorySeparatorChar));
                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(fs);
                return path;
            }

            var encodedPath = EncodePath(path);

            var url = $"{_supabaseUrl}/storage/v1/object/{_bucket}/{encodedPath}";

            using var request = new HttpRequestMessage(HttpMethod.Put, url);

            request.Headers.Add("x-upsert", "true");

            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error reemplazando archivo en Supabase: {response.StatusCode} - {responseText}");
            }

            return path;
        }
        private static string EncodePath(string path)
        {
            return Uri.EscapeDataString(path).Replace("%2F", "/");
        }
    }
}
