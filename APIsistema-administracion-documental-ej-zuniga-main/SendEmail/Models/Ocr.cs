using System;
using System.Collections.Generic;

namespace SendEmail.Models;

public partial class Ocr
{
    public int IdOcr { get; set; }

    public int? IdDoc { get; set; }

    public string? RutaDocumento { get; set; }

    public DateTime? FechaProceso { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Documento? IdDocNavigation { get; set; }
}
