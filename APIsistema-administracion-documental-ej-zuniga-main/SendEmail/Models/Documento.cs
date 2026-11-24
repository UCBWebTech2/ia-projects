using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SendEmail.Models;

public partial class Documento
{
    public int IdDoc { get; set; }

    public string? NombreDocumento { get; set; }

    public string? RutaDocumento { get; set; }

    public string? TipoDocumento { get; set; }

    public int IdUsuario { get; set; }
    public int IdCategoria { get; set; }
    public int? IdProceso { get; set; }
    public bool Habilitado { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    //public string? Estado { get; set; }
    public string? ContenidoDocumento { get; set; }

    // Phase 2: AI prediction / human feedback
    public string? PrediccionIa { get; set; }
    public double? ConfianzaIa { get; set; }
    public string? VersionModelo { get; set; }
    public DateTime? FechaPrediccion { get; set; }
    public int? CategoriaConfirmadaId { get; set; }
    public int? ConfirmadoPor { get; set; }
    public DateTime? FechaConfirmacion { get; set; }

    // Phase 4: gestión avanzada
    public int? AbogadoResponsableId { get; set; }
    public string? ClienteNombre { get; set; }
    public string EstadoDocumento { get; set; } = "ACTIVO";
    public int VersionActual { get; set; } = 1;
    public string? HashContenido { get; set; }

    //public virtual Usuario IdUsuarioNavigation { get; set; }
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Indice> Indices { get; set; } = new List<Indice>();

    public virtual ICollection<Ocr> Ocrs { get; set; } = new List<Ocr>();

    public virtual ICollection<DocumentoEntidad> DocumentoEntidades { get; set; } = new List<DocumentoEntidad>();

    //public virtual ICollection<Categoria> IdCategoria { get; set; } = new List<Categoria>();
    /*public virtual Categoria CategoriaReferencia { get; set; }
    public virtual Proceso ProcesoReferencia { get; set; }*/
    public virtual Categoria CategoriaReferencia { get; set; } = null!;
    public virtual Proceso? ProcesoReferencia { get; set; }
    [NotMapped]
    public IFormFile? Archivo { get; set; }

}
