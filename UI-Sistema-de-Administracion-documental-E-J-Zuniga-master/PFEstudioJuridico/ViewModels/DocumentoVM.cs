namespace PFEstudioJuridico.ViewModels
{
    public class DocumentoVM
    {
        public int IdDoc { get; set; }

        public string? NombreDocumento { get; set; }

        public string? RutaDocumento { get; set; }

        public string? TipoDocumento { get; set; }

        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public int IdCategoria { get; set; }
        public string? NombreCategoria { get; set; }
        public DateTime? FechaCreacion { get; set; }

        public string? Estado { get; set; }
        public bool Habilitado { get; set; }
    }
}
