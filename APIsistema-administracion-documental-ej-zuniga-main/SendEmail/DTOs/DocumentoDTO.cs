using System.ComponentModel.DataAnnotations.Schema;

namespace SendEmail.DTOs
{
    public class DocumentoDTO
    {
        public int IdDoc { get; set; }
        public string? NombreDocumento { get; set; }
        public string? RutaDocumento { get; set; }
        public string? TipoDocumento { get; set; }
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public int IdCategoria { get; set; }
        public int? IdProceso { get; set; }
        public string? NombreCategoria { get; set; }
        public string? NombreProceso { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? ContenidoDocumento { get; set; }
        public bool Habilitado { get; set; }

        // Phase 2: AI prediction / feedback
        public string? PrediccionIa { get; set; }
        public double? ConfianzaIa { get; set; }
        public string? VersionModelo { get; set; }
        public DateTime? FechaPrediccion { get; set; }
        public int? CategoriaConfirmadaId { get; set; }
        public int? ConfirmadoPor { get; set; }
        public DateTime? FechaConfirmacion { get; set; }

        [NotMapped]
        public IFormFile? Archivo { get; set; }
    }
}
