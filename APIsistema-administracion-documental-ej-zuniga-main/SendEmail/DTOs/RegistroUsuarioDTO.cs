namespace APIsEstudioJuridico.DTOs
{
    public class RegistroUsuarioDTO
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string? Rol { get; set; }
    }
}
