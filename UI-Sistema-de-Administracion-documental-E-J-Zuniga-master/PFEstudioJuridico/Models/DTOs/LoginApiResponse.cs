using Newtonsoft.Json;
namespace PFEstudioJuridico.Models.DTOs
{
    public class LoginApiResponse
    {
        [JsonProperty("isSuccess")]
        public bool IsSuccess { get; set; }
        [JsonProperty("message")]
        public string? Message { get; set; }
        [JsonProperty("token")]
        public string? Token { get; set; }
        [JsonProperty("usuario")]
        public UsuarioLoginResponse? Usuario { get; set; }

    }
    public class UsuarioLoginResponse
    {
        [JsonProperty("idUsuario")]
        public int IdUsuario { get; set; }
        [JsonProperty("nombreUsuario")]
        public string? NombreUsuario { get; set; }
        [JsonProperty("correoElectronico")]
        public string? CorreoElectronico { get; set; }
        [JsonProperty("rol")]
        public string? Rol { get; set; }
        [JsonProperty("token")]
        public string? Token { get; set; }
    }
}
