namespace SendEmail.DTOs
{
    public class ProcesoDTO
    {
        public int IdProceso { get; set; }
        public string? NombreProceso { get; set; }
        public string? DescripcionProceso { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string? NombreUsuario { get; set; }
        //public string? Estado { get; set; }
        public bool Habilitado { get; set; }
    }
}
