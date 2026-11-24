using APIsEstudioJuridico.DTOs;
using Microsoft.EntityFrameworkCore;
using SendEmail.Context;
using SendEmail.Models;

namespace APIsEstudioJuridico.Services
{
    public class AuditoriaService
    {
        private readonly AppDbContext _context;

        public AuditoriaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarAuditoriaAsync(int idUsuario, string accion, string descripcion)
        {
            TimeZoneInfo gmtMinus4 = TimeZoneInfo.CreateCustomTimeZone("GMT-4", TimeSpan.FromHours(-4), "GMT-4", "GMT-4");
            var auditoria = new Auditorium
            {
                IdUsuario = idUsuario,
                Accion = accion,
                Descripcion = descripcion,
                Fecha = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, gmtMinus4)
            };

            _context.Auditoria.Add(auditoria);
            await _context.SaveChangesAsync();
        }
        public async Task<List<AuditoriumDTO>> ListaAuditoriasAsync()
        {
            var listaDTO = new List<AuditoriumDTO>();

            var listaDB = await _context.Auditoria
                .Include(a => a.IdUsuarioNavigation)
                .ToListAsync();

            foreach (var item in listaDB)
            {
                listaDTO.Add(new AuditoriumDTO
                {
                    IdAuditoria = item.IdAuditoria,
                    IdUsuario = item.IdUsuario,
                    NombreUsuario = item.IdUsuarioNavigation?.NombreUsuario ?? "Usuario no encontrado",
                    Accion = item.Accion,
                    Descripcion = item.Descripcion,
                    Fecha = item.Fecha
                });
            }
            return listaDTO;
        }

    }
}
