namespace PFEstudioJuridico.Models
{
    public class RespuestaApiAuditoria<T>
    {
        public string Mensaje { get; set; } = string.Empty;
        public T? Response { get; set; }
    }
}
