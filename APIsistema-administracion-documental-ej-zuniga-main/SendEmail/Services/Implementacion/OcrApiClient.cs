using System.Net.Http.Headers;
using System.Security.Claims;
using APIsEstudioJuridico.Services.Contrato;

namespace APIsEstudioJuridico.Services.Implementacion
{
    public class OcrApiClient : IOcrApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IInternalJwtTokenService _tokenService;

        public OcrApiClient(HttpClient httpClient, IInternalJwtTokenService tokenService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
        }

        public async Task<HttpResponseMessage> PostAsync(
            string relativePath,
            HttpContent content,
            ClaimsPrincipal user,
            string permission,
            string operation,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                throw new UnauthorizedAccessException("No se encontró el usuario autenticado.");
            }

            var token = _tokenService.GenerateToken(
                actorUserId,
                new[] { permission },
                operation);

            using var request = new HttpRequestMessage(HttpMethod.Post, relativePath)
            {
                Content = content
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _httpClient.SendAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> GetAsync(
            string relativePath,
            ClaimsPrincipal user,
            string permission,
            string operation,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(actorUserId))
            {
                throw new UnauthorizedAccessException("No se encontró el usuario autenticado.");
            }

            var token = _tokenService.GenerateToken(
                actorUserId,
                new[] { permission },
                operation);

            using var request = new HttpRequestMessage(HttpMethod.Get, relativePath);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _httpClient.SendAsync(request, cancellationToken);
        }
    }
}
