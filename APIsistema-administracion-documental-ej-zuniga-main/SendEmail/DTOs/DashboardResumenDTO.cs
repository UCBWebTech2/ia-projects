namespace APIsEstudioJuridico.DTOs
{
    public class DashboardResumenDTO
    {
        public int TotalDocumentos { get; set; }
        public int DocumentosActivos { get; set; }
        public int DocumentosProcesadosOcr { get; set; }
        public int EntidadesExtraidas { get; set; }
        public int ClasificacionesAutomaticas { get; set; }
        public int BusquedasRealizadas { get; set; }
        public double TiempoPromedioBusquedaMs { get; set; }
        public List<DashboardConteoItemDTO> DocumentosPorCategoria { get; set; } = new();
        public List<DashboardConteoItemDTO> DocumentosPorUsuario { get; set; } = new();
        public List<DashboardSerieMensualDTO> DocumentosPorMes { get; set; } = new();
        public List<DashboardSerieMensualDTO> BusquedasPorMes { get; set; } = new();
    }

    public class DashboardConteoItemDTO
    {
        public string Label { get; set; } = string.Empty;
        public int Valor { get; set; }
    }

    public class DashboardSerieMensualDTO
    {
        public string Mes { get; set; } = string.Empty;
        public int Valor { get; set; }
    }
}
