using Microsoft.EntityFrameworkCore;
using PFEstudioJuridico.Data;  // Asegúrate de que este namespace sea correcto
using PFEstudioJuridico.Models; // Para el modelo de Auditoria
using System;
using System.Threading.Tasks;

namespace PFEstudioJuridico.Services
{
    public class AuditoriaService
    {
       //private readonly AppDBContext _context;

       // public AuditoriaService(AppDBContext context)
       // {
       //     _context = context;
       // }
       // public async Task RegistrarAuditoriaAsync(int idUsuario, string accion, string descripcion)
       // {
       //     var auditoria = new Auditoria
       //     {
       //         IdUsuario = idUsuario,
       //         Accion = accion,
       //         Descripcion = descripcion,
       //         Fecha = DateTime.Now
       //     };

       //     _context.Auditoria.Add(auditoria);
       //     await _context.SaveChangesAsync();
       // }
        //    public async Task<List<Auditoria>> ObtenerAuditoriasAsync()
        //    {
        //        // Verifica que existan entradas en la tabla de Auditoria
        //        return await _context.Auditorias.ToListAsync();
        //    }

        //    public async Task RegistrarAuditoria(int idUsuario, string accion, string descripcion)
        //    {
        //        var auditoria = new Auditoria
        //        {
        //            IdUsuario = idUsuario,
        //            Accion = accion,
        //            Descripcion = descripcion,
        //            Fecha = DateTime.Now
        //        };

        //        _context.Auditorias.Add(auditoria);
        //        await _context.SaveChangesAsync();
        //    }
    }

}
