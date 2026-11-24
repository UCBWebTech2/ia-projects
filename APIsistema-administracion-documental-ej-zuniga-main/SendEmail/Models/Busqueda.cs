using System;

namespace SendEmail.Models;

public partial class Busqueda
{
    public int IdBusqueda { get; set; }
    public int? IdUsuario { get; set; }
    public string TipoBusqueda { get; set; } = string.Empty;
    public string? Query { get; set; }
    public string? FiltrosJson { get; set; }
    public int? Resultados { get; set; }
    public int? TiempoMs { get; set; }
    public DateTime Fecha { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
