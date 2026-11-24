using PFEstudioJuridico.Models.DTOs;

namespace PFEstudioJuridico.ViewModels
{
    public class ResultadoAPI2
    {
        public string? mensaje { get; set; }
        public List<DocumentoDTO>? response { get; set; }
    }
}
