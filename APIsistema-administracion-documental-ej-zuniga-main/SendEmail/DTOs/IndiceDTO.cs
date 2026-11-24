using SendEmail.Models;

namespace APIsEstudioJuridico.DTOs
{
    public class IndiceDTO
    {
        public string? Termino { get; set; }
        public string? TipoTermino { get; set; }
        public int? Frecuencia { get; set; }
        public int? IdDoc { get; set; }
        public virtual Documento? Documento { get; set; }
    }
}
