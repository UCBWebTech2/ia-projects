namespace PFEstudioJuridico.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string? NombreCategoria { get; set; }
        public string? DescripcionCategoria { get; set; }
        public int idUsuario { get; set; }
        public bool Habilitado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public virtual ICollection<Documento> IdDocs { get; set; } = new List<Documento>();
        public virtual Usuario? UsuarioReferencia { get; set; }
    }
}
