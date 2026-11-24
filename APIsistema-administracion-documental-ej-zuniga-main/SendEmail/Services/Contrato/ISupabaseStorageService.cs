using System.IO;
using System.Threading.Tasks;
namespace APIsEstudioJuridico.Services.Contrato
{
    public interface ISupabaseStorageService
    {
        bool IsConfigured { get; }
        string LocalPath { get; }
        Task<string> UploadAsync(Stream stream, string path, string contentType = "application/octet-stream");
        Task<string> CreateSignedUrlAsync(string path, int expiresInSeconds = 3600);
        Task<string> ReplaceAsync(Stream stream, string path, string contentType = "application/octet-stream");
    }
}
