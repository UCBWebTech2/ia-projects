using System.Security.Claims;

namespace APIsEstudioJuridico.Services.Contrato
{
    public interface IOcrApiClient
    {
        Task<HttpResponseMessage> PostAsync(
            string relativePath,
            HttpContent content,
            ClaimsPrincipal user,
            string permission,
            string operation,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> GetAsync(
            string relativePath,
            ClaimsPrincipal user,
            string permission,
            string operation,
            CancellationToken cancellationToken = default);
    }
}
