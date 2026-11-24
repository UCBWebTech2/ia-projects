using System;

namespace SendEmail.Models;

public partial class Notificacion
{
    public int IdNotificacion { get; set; }
    public int? IdUsuario { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
    public bool Leida { get; set; }
    public bool EnviadaCorreo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? MetadataJson { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
