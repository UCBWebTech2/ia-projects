using System;
using System.Collections.Generic;

namespace SendEmail.Models;

public partial class Auditorium
{
    public int IdAuditoria { get; set; }

    public int? IdUsuario { get; set; }

    public string? Accion { get; set; }

    public string? Descripcion { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
