using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Data;
using APIsEstudioJuridico.DTOs;
using SendEmail.Context;
using SendEmail.Models;
using SendEmail.DTOs;
using APIsEstudioJuridico.Services.Contrato;

namespace APIsEstudioJuridico.Services.Implementacion
{
    public class ProcesoService:IProcesoService
    {
        private readonly string _cadenaSql;
        private readonly AppDbContext _context;

        public ProcesoService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _cadenaSql = config.GetConnectionString("CadenaSQL")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'CadenaSQL'.");
        }

        // Obtener todos los procesos
        public async Task<List<ProcesoDTO>> Lista()
        {
            var listaDTO = new List<ProcesoDTO>();
            var listaDB = await _context.Procesos.Include(p => p.UsuarioReferencia).OrderByDescending(p => p.FechaCreacion).ToListAsync();
            foreach (var item in listaDB)
            {
                listaDTO.Add(new ProcesoDTO
                {
                    IdProceso = item.IdProceso,
                    NombreProceso = item.NombreProceso,
                    DescripcionProceso = item.DescripcionProceso,
                    IdUsuario = item.idUsuario,
                    NombreUsuario = item.UsuarioReferencia.NombreUsuario,
                    Habilitado=item.Habilitado
                });
            }
            return listaDTO;
        }
        public async Task<List<ProcesoDTO>> ListaHabilitados()
        {
            var listaDTO = new List<ProcesoDTO>();

            // Filtra los procesos habilitados
            var listaDB = await _context.Procesos
                                        .Include(p => p.UsuarioReferencia)
                                        .Where(p => p.Habilitado)
                                        .OrderByDescending(p => p.FechaCreacion)
                                        .ToListAsync();

            foreach (var item in listaDB)
            {
                listaDTO.Add(new ProcesoDTO
                {
                    IdProceso = item.IdProceso,
                    NombreProceso = item.NombreProceso,
                    DescripcionProceso = item.DescripcionProceso,
                    IdUsuario = item.idUsuario,
                    NombreUsuario = item.UsuarioReferencia?.NombreUsuario,
                    Habilitado = item.Habilitado
                });
            }

            return listaDTO;
        }

        public async Task<bool> CambiarEstado(int id)
        {
            var procesoDB = await _context.Procesos.AsNoTracking().FirstOrDefaultAsync(p => p.IdProceso == id);

            if (procesoDB == null)
            {
                return false;
            }

            procesoDB.Habilitado = !procesoDB.Habilitado;

            try
            {
                _context.Procesos.Update(procesoDB);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error al actualizar el estado en la base de datos", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al actualizar el estado en la base de datos", ex);
            }
        }

        // Obtener un proceso por ID
        public async Task<ProcesoDTO?> ObtenerPorId(int id)
        {
            var proceso = await _context.Procesos.FindAsync(id);
            if (proceso == null)
            {
                return null;
            }

            return new ProcesoDTO
            {
                IdProceso = proceso.IdProceso,
                NombreProceso = proceso.NombreProceso,
                DescripcionProceso = proceso.DescripcionProceso,
                IdUsuario = proceso.idUsuario
                //Estado = proceso.Estado
            };
        }

        // Crear un nuevo proceso
        public async Task CrearProcesoAsync(ProcesoDTO procesoDTO)
        {
            // Validación adicional si es necesario
            if (string.IsNullOrEmpty(procesoDTO.NombreProceso) || string.IsNullOrEmpty(procesoDTO.DescripcionProceso))
            {
                throw new ArgumentException("El nombre y la descripción del proceso son obligatorios.");
            }

            using (var conexion = new SqlConnection(_cadenaSql))
            {
                await conexion.OpenAsync();
                using (var transaction = conexion.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SqlCommand("InsertarProceso", conexion, transaction);
                        cmd.Parameters.AddWithValue("@nombreProceso", procesoDTO.NombreProceso);
                        cmd.Parameters.AddWithValue("@descripcionProceso", procesoDTO.DescripcionProceso);
                        cmd.Parameters.AddWithValue("@idUsuario", procesoDTO.IdUsuario);
                        cmd.Parameters.Add("@habilitado", SqlDbType.Bit).Value = procesoDTO.Habilitado;
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Ejecuta el procedimiento almacenado
                        await cmd.ExecuteNonQueryAsync();

                        // Confirmar la transacción si todo es exitoso
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Revertir la transacción en caso de error
                        transaction.Rollback();
                        throw new Exception("Error al insertar el proceso: " + ex.Message);
                    }
                }
            }
        }

        // Actualizar un proceso existente
        public async Task<bool> ActualizarProceso(int id, ProcesoDTO procesoDTO)
        {
            var procesoExistente = await _context.Procesos.AsNoTracking().FirstOrDefaultAsync(c => c.IdProceso == id);
            if (procesoExistente == null)
            {
                return false;
            }
            if (procesoExistente.NombreProceso == procesoDTO.NombreProceso &&
                procesoExistente.DescripcionProceso == procesoDTO.DescripcionProceso &&
                procesoExistente.Habilitado == procesoDTO.Habilitado)
            {
                return true; // No es necesario realizar actualizaciones
            }

            // Actualiza los campos necesarios
            procesoExistente.NombreProceso = procesoDTO.NombreProceso;
            procesoExistente.DescripcionProceso = procesoDTO.DescripcionProceso;
            procesoExistente.FechaModificacion = DateTime.Now;
            procesoExistente.Habilitado = procesoDTO.Habilitado;
            try
            {
                _context.Procesos.Update(procesoExistente);
                await _context.SaveChangesAsync();
                return true; // Indica éxito
            }
            catch (DbUpdateException ex)
            {
                // Log o manejo de error específico de la base de datos
                throw new Exception("Error al actualizar el proceso en la base de datos", ex);
            }
            catch (Exception ex)
            {
                // Captura errores inesperados
                throw new Exception("Error inesperado al actualizar el proceso", ex);
            }
        }

        // Eliminar un proceso
        public async Task<bool> EliminarProceso(int id)
        {
            var proceso = await _context.Procesos.FindAsync(id);
            if (proceso == null)
            {
                return false;
            }

            _context.Procesos.Remove(proceso);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
