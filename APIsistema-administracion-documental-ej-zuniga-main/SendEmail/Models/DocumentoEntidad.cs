using System;

namespace SendEmail.Models;

public partial class DocumentoEntidad
{
    public int IdEntidad { get; set; }
    public int IdDoc { get; set; }
    public string TipoEntidad { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? ValorNormalizado { get; set; }
    public string Fuente { get; set; } = "NER";
    public double? Confianza { get; set; }
    public DateTime FechaCreacion { get; set; }

    public virtual Documento? IdDocNavigation { get; set; }
}
