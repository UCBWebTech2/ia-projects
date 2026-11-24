using Microsoft.EntityFrameworkCore;
using SendEmail.Context;
using SendEmail.Models;
using SendEmail.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data.SqlClient;
using System.Data;
//using Firebase.Auth;
namespace APIsEstudioJuridico.Services
{
    public class CategoriaService
    {
        private readonly string _rutaServidor;
        private readonly string _cadenaSql;
        //private readonly FirebaseAuthClient _firebaseAuthClient;
        private readonly AppDbContext _context;
        public CategoriaService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            //_rutaServidor = config.GetSection("Configuracion").GetSection("RutaServidor").Value;
            //_cadenaSql = config.GetConnectionString("CadenaSQL");
            _rutaServidor = config["Configuracion:RutaServidor"]
                ?? throw new InvalidOperationException("No se encontró la configuración 'Configuracion:RutaServidor'.");

            _cadenaSql = config.GetConnectionString("CadenaSQL")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'CadenaSQL'.");
        }
        public async Task<List<CategoriaDTO>>Lista()
        {
            var listaDTO = new List<CategoriaDTO>();
            var listaDB = await _context.Categorias.Include(c => c.UsuarioReferencia).ToListAsync();
            foreach (var item in listaDB)
            {
                listaDTO.Add(new CategoriaDTO
                {
                    IdCategoria = item.IdCategoria,
                    NombreCategoria = item.NombreCategoria,
                    DescripcionCategoria = item.DescripcionCategoria,
                    IdUsuario = item.idUsuario,
                    NombreUsuario = item.UsuarioReferencia.NombreUsuario,
                    Habilitado = item.Habilitado
                });
            }
            return listaDTO;
        }
        public async Task<List<CategoriaDTO>> ListaHabilitadas()
        {
            var listaDTO = new List<CategoriaDTO>();

            // Filtrar solo las categorías habilitadas
            var listaDB = await _context.Categorias
                .Include(c => c.UsuarioReferencia)
                .Where(c => c.Habilitado) // Solo categorías habilitadas
                .ToListAsync();

            foreach (var item in listaDB)
            {
                listaDTO.Add(new CategoriaDTO
                {
                    IdCategoria = item.IdCategoria,
                    NombreCategoria = item.NombreCategoria,
                    DescripcionCategoria = item.DescripcionCategoria,
                    IdUsuario = item.idUsuario,
                    NombreUsuario = item.UsuarioReferencia.NombreUsuario,
                    Habilitado = item.Habilitado
                });
            }
            return listaDTO;
        }

        public async Task<CategoriaDTO?> ObtenerPorId(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return null;
            }

            return new CategoriaDTO
            {
                IdCategoria = categoria.IdCategoria,
                NombreCategoria = categoria.NombreCategoria,
                DescripcionCategoria = categoria.DescripcionCategoria,
                IdUsuario = categoria.idUsuario
            };
        }
        public async Task<bool> CambiarEstado(int id)
        {
            var categoriaDB = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.IdCategoria == id);

            if (categoriaDB == null)
            {
                return false;
            }

            categoriaDB.Habilitado = !categoriaDB.Habilitado;

            try
            {
                _context.Categorias.Update(categoriaDB);
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

        public async Task CrearCategoriaAsync(CategoriaDTO categoriaDTO)
        {
            // Validación adicional si es necesario
            if (string.IsNullOrEmpty(categoriaDTO.NombreCategoria) || string.IsNullOrEmpty(categoriaDTO.DescripcionCategoria))
            {
                throw new ArgumentException("El nombre y la descripción son obligatorios.");
            }

            using (var conexion = new SqlConnection(_cadenaSql))
            {
                await conexion.OpenAsync();
                using (var transaction = conexion.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SqlCommand("InsertarCategoria", conexion, transaction);
                        cmd.Parameters.AddWithValue("@nombreCategoria", categoriaDTO.NombreCategoria);
                        cmd.Parameters.AddWithValue("@descripcionCategoria", categoriaDTO.DescripcionCategoria);
                        cmd.Parameters.AddWithValue("@idUsuario", categoriaDTO.IdUsuario);
                        cmd.Parameters.Add("@habilitado", SqlDbType.Bit).Value = categoriaDTO.Habilitado;
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
                        throw new Exception("Error al insertar la categoria: " + ex.Message);
                    }
                }
            }
        }


        public async Task<bool> CrearCategoria(string nombre, string descripcion)
        {
            var parameterNombre = new SqlParameter("@nombreCategoria", nombre);
            var parameterDescripcion = new SqlParameter("@descripcionCategoria", descripcion);

            using (var conexion = new SqlConnection(_cadenaSql))
            {
                await conexion.OpenAsync();
                using (var transaction = conexion.BeginTransaction())
                {
                    try
                    {
                        var cmd = new SqlCommand("InsertarCategoria", conexion, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(parameterNombre);
                        cmd.Parameters.Add(parameterDescripcion);

                        var result = await cmd.ExecuteNonQueryAsync();

                        if (result > 0)
                        {
                            transaction.Commit();
                            return true;
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al crear categoría: {ex.Message}");
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return false;
        }



        public async Task<bool> Actualizar(int id, CategoriaDTO categoriaDTO)
        {
            // Busca la categoría en la base de datos
            var categoria = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.IdCategoria == id);
            if (categoria == null)
            {
                return false; // Retorna falso si no existe
            }

            // Verifica si hay cambios en los datos antes de actualizar
            if (categoria.NombreCategoria == categoriaDTO.NombreCategoria &&
                categoria.DescripcionCategoria == categoriaDTO.DescripcionCategoria &&
                categoria.Habilitado == categoriaDTO.Habilitado)
            {
                return true; // No es necesario realizar actualizaciones
            }

            // Actualiza los campos necesarios
            categoria.NombreCategoria = categoriaDTO.NombreCategoria;
            categoria.DescripcionCategoria = categoriaDTO.DescripcionCategoria;
            categoria.FechaModificacion = DateTime.Now;
            categoria.Habilitado = categoriaDTO.Habilitado;
            try
            {
                _context.Categorias.Update(categoria);
                await _context.SaveChangesAsync();
                return true; // Indica éxito
            }
            catch (DbUpdateException ex)
            {
                // Log o manejo de error específico de la base de datos
                throw new Exception("Error al actualizar la categoría en la base de datos", ex);
            }
            catch (Exception ex)
            {
                // Captura errores inesperados
                throw new Exception("Error inesperado al actualizar la categoría", ex);
            }
        }

    }
}
