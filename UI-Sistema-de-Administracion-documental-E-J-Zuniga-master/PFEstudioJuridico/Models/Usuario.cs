namespace PFEstudioJuridico.Models
{
    public class Usuario
    {
        public int idUsuario { get; set; }
        public string nombreUsuario { get; set; } = string.Empty;
        public string correoElectronico {  get; set; } = string.Empty;
        public string? contrasenia { get; set; }
        public string Rol { get; set; } = string.Empty;
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }

        //public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();
        public virtual ICollection<Documento> DocumentosRef { get; set; } = new List<Documento>();
    }
}
