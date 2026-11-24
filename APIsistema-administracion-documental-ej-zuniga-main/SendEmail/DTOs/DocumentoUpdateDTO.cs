namespace SendEmail.DTOs
{
    public class DocumentoUpdateDTO
    {
        public IFormFile? Archivo { get; set; }
        public string NombreDocumento { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        //public string Estado { get; set; }
    }
}
