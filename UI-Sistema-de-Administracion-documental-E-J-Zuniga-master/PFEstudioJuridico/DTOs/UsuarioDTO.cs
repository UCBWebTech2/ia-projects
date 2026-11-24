namespace PFEstudioJuridico.DTOs
{
    public class UsuarioDTO
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Contrasenia { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string? ConfirmarClave { get; set; }
        public bool Restablecer { get; set; }
        public bool Confirmado { get; set; }
        public string Token { get; set; } = string.Empty;
        //public DateTime? FechaCreacion { get; set; }

        //public DateTime? FechaModificacion { get; set; }
    }
}
