using System;
using System.Collections.Generic;

namespace SendEmail.ViewModels;

public partial class UsuarioVM
{
    public int IdUsuario { get; set; }

    public string? NombreUsuario { get; set; }

    public string? CorreoElectronico { get; set; }

    public string? Contrasenia { get; set; }

    public string? Rol { get; set; }
    public string? Token { get; set; }

    //public DateTime? FechaCreacion { get; set; }

    //public DateTime? FechaModificacion { get; set; }

}
