namespace APIsEstudioJuridico.Models
{
    public class FiltroBusqueda
    {
        public string? Query { get; set; }
        public string? TipoDocumento { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? Categoria { get; set; }
        public int? Proceso { get; set; }
        public int? IdUsuario { get; set; }
        public int? AbogadoResponsableId { get; set; }
        public string? ClienteNombre { get; set; }
        public string? EstadoDocumento { get; set; }
        public string? TipoEntidad { get; set; }
        public string? ValorEntidad { get; set; }
    }
}
