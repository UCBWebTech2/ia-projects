using System;
using System.Collections.Generic;

namespace SendEmail.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }
    public string? NombreUsuario { get; set; }
    public string? CorreoElectronico { get; set; }
    public string? Contrasenia { get; set; }
    public string? Rol { get; set; }
    //public string ConfirmarClave { get; set; }
    public bool Restablecer { get; set; }
    public bool Confirmado { get; set; }
    //public string Token { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? Token { get; set; }
    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();
    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
    public virtual ICollection<Proceso> Procesos { get; set; } = new List<Proceso>();
    public virtual ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
}
