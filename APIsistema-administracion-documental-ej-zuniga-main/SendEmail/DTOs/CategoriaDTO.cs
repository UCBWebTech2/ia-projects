namespace SendEmail.DTOs
{
    public class CategoriaDTO
    {
        public int IdCategoria { get; set; }
        public string? NombreCategoria { get; set; }
        public string? DescripcionCategoria { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string? NombreUsuario { get; set; }
        public bool Habilitado { get; set; }
    }
}
