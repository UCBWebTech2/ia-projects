namespace SendEmail.DTOs;

public class ModelRegistryDTO
{
    public int IdModelRegistry { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? ModelPath { get; set; }
    public string? DatasetHash { get; set; }
    public string? DatasetPath { get; set; }
    public double? Accuracy { get; set; }
    public double? MacroF1 { get; set; }
    public string? MetricsJson { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? Notas { get; set; }
}
