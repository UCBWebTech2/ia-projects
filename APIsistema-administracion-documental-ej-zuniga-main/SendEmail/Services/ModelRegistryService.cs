using Microsoft.EntityFrameworkCore;
using SendEmail.Context;
using SendEmail.DTOs;
using SendEmail.Models;

namespace APIsEstudioJuridico.Services;

public class ModelRegistryService
{
    private readonly AppDbContext _context;

    public ModelRegistryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ModelRegistryDTO>> ListarAsync()
    {
        return await _context.ModelRegistryEntries
            .OrderByDescending(x => x.FechaCreacion)
            .Select(x => ToDto(x))
            .ToListAsync();
    }

    public async Task<ModelRegistryDTO?> ObtenerActivoAsync()
    {
        var entity = await _context.ModelRegistryEntries
            .Where(x => x.Activo)
            .OrderByDescending(x => x.FechaCreacion)
            .FirstOrDefaultAsync();
        return entity == null ? null : ToDto(entity);
    }

    public async Task<ModelRegistryDTO> RegistrarAsync(ModelRegistryDTO dto, bool activar = false)
    {
        var entity = new ModelRegistryEntry
        {
            Version = dto.Version,
            ModelPath = dto.ModelPath,
            DatasetHash = dto.DatasetHash,
            DatasetPath = dto.DatasetPath,
            Accuracy = dto.Accuracy,
            MacroF1 = dto.MacroF1,
            MetricsJson = dto.MetricsJson,
            Activo = false,
            FechaCreacion = DateTime.UtcNow,
            Notas = dto.Notas,
        };

        if (activar)
        {
            var activos = await _context.ModelRegistryEntries.Where(x => x.Activo).ToListAsync();
            foreach (var a in activos) a.Activo = false;
            entity.Activo = true;
        }

        _context.ModelRegistryEntries.Add(entity);
        await _context.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> ActivarAsync(int id)
    {
        var target = await _context.ModelRegistryEntries.FindAsync(id);
        if (target == null) return false;
        var activos = await _context.ModelRegistryEntries.Where(x => x.Activo).ToListAsync();
        foreach (var a in activos) a.Activo = false;
        target.Activo = true;
        await _context.SaveChangesAsync();
        return true;
    }

    private static ModelRegistryDTO ToDto(ModelRegistryEntry x) => new()
    {
        IdModelRegistry = x.IdModelRegistry,
        Version = x.Version,
        ModelPath = x.ModelPath,
        DatasetHash = x.DatasetHash,
        DatasetPath = x.DatasetPath,
        Accuracy = x.Accuracy,
        MacroF1 = x.MacroF1,
        MetricsJson = x.MetricsJson,
        Activo = x.Activo,
        FechaCreacion = x.FechaCreacion,
        Notas = x.Notas,
    };
}
