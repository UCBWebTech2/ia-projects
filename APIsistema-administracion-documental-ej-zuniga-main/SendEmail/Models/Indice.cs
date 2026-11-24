using System;
using System.Collections.Generic;

namespace SendEmail.Models;

public partial class Indice
{
    public int IdIndice { get; set; }
    public int? IdDoc { get; set; }
    public string? Termino { get; set; }
    public int? Frecuencia { get; set; }
    public string? TipoTermino { get; set; }
    public virtual Documento? IdDocNavigation { get; set; }
}
