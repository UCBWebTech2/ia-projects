namespace APIsEstudioJuridico.DTOs
{
    public class DocumentoDetalleDTO
    {
        public int IdDocumento { get; set; }
        public string NombreDocumento { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }
        public bool Habilitado { get; set; }
        public string? PrediccionIa { get; set; }
        public double? ConfianzaIa { get; set; }
        public string? VersionModelo { get; set; }
        public int? CategoriaConfirmadaId { get; set; }
        public List<IndiceDTO> Indices { get; set; } = new List<IndiceDTO>();
        public List<IndiceDTO> Entidades { get; set; } = new List<IndiceDTO>();
    }
}
