using Humanizer;
using System.ComponentModel.DataAnnotations.Schema;

namespace PFEstudioJuridico.Models
{
    public class Documento
    {
        public int IdDoc { get; set; }

        public string? NombreDocumento { get; set; }

        public string? RutaDocumento { get; set; }

        public string? TipoDocumento { get; set; }

        public int IdUsuario { get; set; }
        public int IdCategoria { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public DateTime? FechaModificacion { get; set; }

        //public string? Estado { get; set; }
        public virtual Usuario? UsuarioReferencia { get; set; }
        public virtual ICollection<Indice> Indices { get; set; } = new List<Indice>();

        //public virtual ICollection<Ocr> Ocrs { get; set; } = new List<Ocr>();

        //public virtual ICollection<Categoria> IdCategoria { get; set; } = new List<Categoria>();

        public virtual Categoria? CategoriaReferencia { get; set; }
        [NotMapped]
        public IFormFile? Archivo { get; set; }
    }
}
