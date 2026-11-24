using APIsEstudioJuridico.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendEmail.Context;

namespace SendEmail.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Abogado,Administrador")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Resumen")]
        public async Task<IActionResult> Resumen()
        {
            var dto = new DashboardResumenDTO();

            dto.TotalDocumentos = await _context.Documentos.CountAsync();
            dto.DocumentosActivos = await _context.Documentos.CountAsync(d => d.Habilitado);
            dto.DocumentosProcesadosOcr = await _context.Ocrs.Select(o => o.IdDoc).Distinct().CountAsync();
            dto.EntidadesExtraidas = await _context.Indices.CountAsync();
            dto.ClasificacionesAutomaticas = await _context.Documentos.CountAsync(d => d.PrediccionIa != null);

            dto.DocumentosPorCategoria = await _context.Documentos
                .Include(d => d.CategoriaReferencia)
                .GroupBy(d => d.CategoriaReferencia != null ? d.CategoriaReferencia.NombreCategoria : "Sin categoría")
                .Select(g => new DashboardConteoItemDTO { Label = g.Key ?? "Sin categoría", Valor = g.Count() })
                .OrderByDescending(x => x.Valor).Take(10).ToListAsync();

            dto.DocumentosPorUsuario = await _context.Documentos
                .Include(d => d.IdUsuarioNavigation)
                .GroupBy(d => d.IdUsuarioNavigation != null ? d.IdUsuarioNavigation.NombreUsuario : "Sin usuario")
                .Select(g => new DashboardConteoItemDTO { Label = g.Key ?? "Sin usuario", Valor = g.Count() })
                .OrderByDescending(x => x.Valor).Take(10).ToListAsync();

            var desde = DateTime.UtcNow.AddMonths(-11);
            var inicioMes = new DateTime(desde.Year, desde.Month, 1);
            var docsMes = await _context.Documentos
                .Where(d => d.FechaCreacion >= inicioMes)
                .GroupBy(d => new { d.FechaCreacion!.Value.Year, d.FechaCreacion!.Value.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Valor = g.Count() })
                .ToListAsync();
            dto.DocumentosPorMes = docsMes
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .Select(x => new DashboardSerieMensualDTO { Mes = $"{x.Year}-{x.Month:00}", Valor = x.Valor })
                .ToList();

            try
            {
                dto.BusquedasRealizadas = await _context.Busquedas.CountAsync();
                dto.TiempoPromedioBusquedaMs = await _context.Busquedas.AnyAsync()
                    ? await _context.Busquedas.AverageAsync(b => (double)(b.TiempoMs ?? 0))
                    : 0;
                var busMes = await _context.Busquedas
                    .Where(b => b.Fecha >= inicioMes)
                    .GroupBy(b => new { b.Fecha.Year, b.Fecha.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Valor = g.Count() })
                    .ToListAsync();
                dto.BusquedasPorMes = busMes
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .Select(x => new DashboardSerieMensualDTO { Mes = $"{x.Year}-{x.Month:00}", Valor = x.Valor })
                    .ToList();
            }
            catch
            {
                // Tablas de expansión pueden no existir aún.
            }

            return Ok(dto);
        }
    }
}
