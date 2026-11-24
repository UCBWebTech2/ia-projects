namespace APIsEstudioJuridico.DTOs
{
    public class AuditoriumDTO
    {
        public int IdAuditoria { get; set; }
        public int? IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public string? Accion { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? Fecha { get; set; }
    }
}
