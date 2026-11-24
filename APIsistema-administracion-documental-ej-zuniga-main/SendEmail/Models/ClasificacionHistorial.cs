using System;

namespace SendEmail.Models;

public partial class ClasificacionHistorial
{
    public int IdHistorial { get; set; }
    public int IdDoc { get; set; }
    public int? CategoriaAnteriorId { get; set; }
    public int? CategoriaNuevaId { get; set; }
    public string? PrediccionIa { get; set; }
    public double? ConfianzaIa { get; set; }
    public string? VersionModelo { get; set; }
    public string Origen { get; set; } = string.Empty;
    public int? IdUsuario { get; set; }
    public DateTime Fecha { get; set; }
    public string? Comentario { get; set; }

    public virtual Documento? IdDocNavigation { get; set; }
    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
