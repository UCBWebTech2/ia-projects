using System.ComponentModel.DataAnnotations;

namespace PFEstudioJuridico.Models
{
    public class Auditoria
    {
        //[Key]
        public int IdAuditoria { get; set; }
        public int IdUsuario { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
