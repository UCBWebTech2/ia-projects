namespace PFEstudioJuridico.Models
{
    public class Proceso
    {
        public int IdProceso { get; set; }
        public string? NombreProceso { get; set; }
        public string? DescripcionProceso { get; set; }
        public int idUsuario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Habilitado { get; set; }
        public virtual ICollection<Documento> IdDocs { get; set; } = new List<Documento>();
        public virtual Usuario? UsuarioReferencia { get; set; }
    }
}
