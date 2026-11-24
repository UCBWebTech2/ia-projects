namespace SendEmail.Models;

public partial class MapeoCategoria
{
    public int IdMapeoCategoria { get; set; }
    public string ModelClassName { get; set; } = string.Empty;
    public int IdCategoria { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public virtual Categoria? Categoria { get; set; }
}
