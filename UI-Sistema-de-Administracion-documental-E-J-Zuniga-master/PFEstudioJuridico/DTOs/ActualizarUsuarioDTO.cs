namespace PFEstudioJuridico.DTOs
{
    public class ActualizarUsuarioDTO
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Confirmar { get; set; }
    }
}
