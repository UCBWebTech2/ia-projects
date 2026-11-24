namespace SendEmail.DTOs;

public class ConfirmarPrediccionDTO
{
    public int IdDocumento { get; set; }
    public int IdCategoriaConfirmada { get; set; }
    public string? Comentario { get; set; }
}
