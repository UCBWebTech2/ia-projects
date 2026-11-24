using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SendEmail.Models;
using APIsEstudioJuridico.Services.Contrato;
using APIsEstudioJuridico.DTOs;
using SendEmail.Context;
using SendEmail.DTOs;
using SendEmail.Custom;

namespace APIsEstudioJuridico.Services.Implementacion
{
    public class UsuarioService:IUsuarioService
    {
        private readonly AppDbContext _dbContext;
        public UsuarioService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Usuario?> GetUsuario(string correo, string clave)
        {
            //Usuario usuario_encontrado = await _dbContext.Usuarios.Where(u => u.CorreoElectronico == correo && u.Contrasenia == clave)
            //     .FirstOrDefaultAsync();

            //return usuario_encontrado;
            return await _dbContext.Usuarios
            .FirstOrDefaultAsync(u =>
                u.CorreoElectronico == correo &&
                u.Contrasenia == clave
            );
        }
        public async Task<Usuario?> ObtenerUsuario(string correo)
        {
            //Usuario usuario_encontrado = await _dbContext.Usuarios.Where(u => u.CorreoElectronico == correo)
            //     .FirstOrDefaultAsync();

            //return usuario_encontrado;
            return await _dbContext.Usuarios
            .FirstOrDefaultAsync(u => u.CorreoElectronico == correo);
        }

        public async Task<Usuario> Obtener(string correo)
        {
            //if (string.IsNullOrEmpty(correo))
            //{
            //    throw new ArgumentException("El correo no puede ser nulo o vacío.", nameof(correo));
            //}

            //try
            //{
            //    Usuario usuarioEncontrado = await _dbContext.Usuarios
            //        .AsNoTracking()
            //        .FirstOrDefaultAsync(u => u.CorreoElectronico == correo)
            //        ?? throw new ArgumentException("El usuario no existe.", nameof(correo));

            //    return usuarioEncontrado;
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("Ocurrió un error al obtener el usuario.", ex);
            //}
            if (string.IsNullOrWhiteSpace(correo))
            {
                throw new ArgumentException("El correo no puede ser nulo o vacío.", nameof(correo));
            }

            var usuarioEncontrado = await _dbContext.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.CorreoElectronico == correo);

            if (usuarioEncontrado == null)
            {
                throw new ArgumentException("El usuario no existe.", nameof(correo));
            }

            return usuarioEncontrado;
        }
        public async Task<Usuario?> ObtenerUsuarioPorToken(string tokenRestablecimiento)
        {
            //var usuario = await _dbContext.Usuarios
            //    .FirstOrDefaultAsync(u => u.Token == tokenRestablecimiento);

            //if (usuario == null)
            //{
            //    return null;
            //}

            //return usuario;
            return await _dbContext.Usuarios
                .FirstOrDefaultAsync(u => u.Token == tokenRestablecimiento);
        }
        public async Task<String?> ObtenerCorreoPorToken(string token)
        {
            //var usuario = await _dbContext.Usuarios
            //    .FirstOrDefaultAsync(u => u.Token == token);

            //if (usuario != null)
            //{
            //    return usuario.CorreoElectronico;
            //}

            //return null;
            var usuario = await _dbContext.Usuarios
               .FirstOrDefaultAsync(u => u.Token == token);

            return usuario?.CorreoElectronico;
        }
        public async Task<Usuario> SaveUsuario(UsuarioDTO modelo)
        {
            var modeloUsuario = new Usuario
            {
                NombreUsuario = modelo.NombreUsuario,
                CorreoElectronico = modelo.CorreoElectronico,
                Contrasenia = modelo.Contrasenia,
                Rol = modelo.Rol,
                FechaCreacion = DateTime.Now,
                FechaModificacion = DateTime.Now,
                Token = modelo.Token??Utilidades.GenerarToken()
            };
            _dbContext.Usuarios.Add(modeloUsuario);
            await _dbContext.SaveChangesAsync();
            return modeloUsuario;
        }
        public async Task<Usuario> SaveUsuario2(UsuarioDTO modelo)
        {
            var modeloUsuario = new Usuario
            {
                NombreUsuario = modelo.NombreUsuario,
                CorreoElectronico = modelo.CorreoElectronico,
                Contrasenia = modelo.Contrasenia,
                Rol = modelo.Rol,
                FechaCreacion = DateTime.Now,
                FechaModificacion = DateTime.Now,
                Token = modelo.Token ?? Utilidades.GenerarToken(),
                Confirmado = true,
                Restablecer = true
                
            };
            _dbContext.Usuarios.Add(modeloUsuario);
            await _dbContext.SaveChangesAsync();
            return modeloUsuario;
        }
        public async Task<bool> CambiarEstadoConfirmado(int id)
        {
            var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return false; // Usuario no encontrado
            }

            usuario.Confirmado = !usuario.Confirmado; // Cambiar el estado
            usuario.FechaModificacion = DateTime.Now; // Actualizar la fecha de modificación

            try
            {
                _dbContext.Usuarios.Update(usuario);
                await _dbContext.SaveChangesAsync();
                return true; // Éxito
            }
            catch (DbUpdateException ex)
            {
                // Manejar errores de base de datos
                throw new Exception("Error al cambiar el estado en la base de datos.", ex);
            }
            catch (Exception ex)
            {
                // Manejar errores inesperados
                throw new Exception("Error inesperado al cambiar el estado.", ex);
            }
        }

        public async Task<bool> ActualizarUsuario(Usuario usuario)
        {
            try
            {
                _dbContext.Usuarios.Update(usuario);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        //public async Task<Usuario> ObtenerUsuarioPorId(int idUsuario)
        //{
        //    return await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        //}
        public async Task<Usuario?> ObtenerUsuarioPorId(int idUsuario)
        {
            return await _dbContext.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        }
        public async Task<bool> Actualizar(int restablecer, string contrasenia, string tokenRestablecimiento)
        {
            if (restablecer != 0 && restablecer != 1)
            {
                throw new ArgumentException("El valor de restablecer debe ser 0 o 1.", nameof(restablecer));
            }

            bool respuesta = false;

            try
            {
                Usuario? usuario_encontrado = await _dbContext.Usuarios
                    .FirstOrDefaultAsync(u => u.Token == tokenRestablecimiento);

                if (usuario_encontrado == null)
                {
                    return respuesta;
                }

                // Actualizar el campo de restablecer si corresponde
                if (restablecer == 1)
                {
                    usuario_encontrado.Restablecer = true;
                }
                else
                {
                    usuario_encontrado.Restablecer = false;
                }

                usuario_encontrado.Contrasenia = contrasenia;
                usuario_encontrado.FechaModificacion = DateTime.Now;
                usuario_encontrado.Token = tokenRestablecimiento;
                usuario_encontrado.Confirmado = true;

                _dbContext.Usuarios.Update(usuario_encontrado);
                await _dbContext.SaveChangesAsync();

                respuesta = true; // Indicar que la operación fue exitosa
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error al restablecer la contraseña.", ex);
            }

            return respuesta;
        }
        public async Task<bool> Restablecer(int restablecer, string contrasenia, string tokenRestablecimiento)
        {
            if (restablecer != 0 && restablecer != 1)
            {
                throw new ArgumentException("El valor de restablecer debe ser 0 o 1.", nameof(restablecer));
            }

            bool respuesta = false;

            try
            {
                Usuario? usuario_encontrado = await _dbContext.Usuarios
                    .FirstOrDefaultAsync(u => u.Token == tokenRestablecimiento);

                if (usuario_encontrado == null)
                {
                    return respuesta;
                }

                if(restablecer == 1)
                {
                    usuario_encontrado.Restablecer = true;
                }
                else
                {
                    usuario_encontrado.Restablecer = false;
                }
                
                usuario_encontrado.Contrasenia = contrasenia;
                usuario_encontrado.FechaModificacion = DateTime.Now;
                usuario_encontrado.Token = tokenRestablecimiento;

                _dbContext.Usuarios.Update(usuario_encontrado);
                await _dbContext.SaveChangesAsync();

                respuesta = true; // Indicar que la operación fue exitosa
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error al restablecer la contraseña.", ex);
            }

            return respuesta;
        }

        public async Task<bool> Confirmar(int idUsuario)
        {
            bool respuesta = false;
            Usuario? usuario_encontrado = await _dbContext.Usuarios.Where(u => u.IdUsuario == idUsuario)
                 .FirstOrDefaultAsync();
            if(usuario_encontrado == null)
            {
                return respuesta;
            }
            usuario_encontrado.Confirmado = true;
            usuario_encontrado.FechaModificacion = DateTime.Now;
            
            _dbContext.Usuarios.Update(usuario_encontrado);
            await _dbContext.SaveChangesAsync();
            respuesta = true;
            return respuesta;
        }
        public async Task<bool> ConfirmarPorTokenConfirmado(string token)
        {
            bool respuesta = false;
            Usuario? usuario_encontrado = await _dbContext.Usuarios
                .Where(u => u.Token == token) // Verificamos que el token coincide y que no esté ya confirmado
                .FirstOrDefaultAsync();

            if (usuario_encontrado == null)
            {
                return respuesta;
            }

            usuario_encontrado.Confirmado = true;
            usuario_encontrado.FechaModificacion = DateTime.Now;

            _dbContext.Usuarios.Update(usuario_encontrado);
            await _dbContext.SaveChangesAsync();
            respuesta = true;
            return respuesta;
        }
        public async Task<bool> ConfirmarPorToken(string token)
        {
            bool respuesta = false;
            Usuario? usuario_encontrado = await _dbContext.Usuarios
                .Where(u => u.Token == token && !u.Confirmado) // Verificamos que el token coincide y que no esté ya confirmado
                .FirstOrDefaultAsync();

            if (usuario_encontrado == null)
            {
                return respuesta;
            }

            usuario_encontrado.Confirmado = true;
            usuario_encontrado.FechaModificacion = DateTime.Now;

            _dbContext.Usuarios.Update(usuario_encontrado);
            await _dbContext.SaveChangesAsync();
            respuesta = true;
            return respuesta;
        }
        public async Task<Usuario> RegistrarUsuario(UsuarioDTO modelo)
        {
            var modeloUsuario = new Usuario
            {
                NombreUsuario = modelo.NombreUsuario,
                CorreoElectronico = modelo.CorreoElectronico,
                Contrasenia = modelo.Contrasenia,
                Rol = modelo.Rol,
                FechaCreacion = DateTime.Now,
                FechaModificacion = DateTime.Now
            };
            _dbContext.Usuarios.Add(modeloUsuario);
            await _dbContext.SaveChangesAsync();
            return modeloUsuario;
        }
        public async Task<List<UsuarioDTO2>> ObtenerTodosLosUsuarios()
        {
            var listaDTO = new List<UsuarioDTO2>();
            foreach (var item in await _dbContext.Usuarios.ToListAsync())
            {
                listaDTO.Add(new UsuarioDTO2
                {
                    IdUsuario=item.IdUsuario,
                    NombreUsuario = item.NombreUsuario ?? string.Empty,
                    CorreoElectronico = item.CorreoElectronico ?? string.Empty,
                    Rol = item.Rol,
                    Confirmado = item.Confirmado
                });
            }
            return listaDTO;
        }
        public async Task<List<UsuarioDTO2>> ObtenerUsuariosConRolAbogado()
        {
            var listaDTO = new List<UsuarioDTO2>();

            // Filtrar usuarios con el rol "Abogado"
            var usuariosAbogados = await _dbContext.Usuarios
                .Where(u => u.Rol == "Abogado")
                .ToListAsync();

            // Mapear los datos a UsuarioDTO2
            foreach (var item in usuariosAbogados)
            {
                listaDTO.Add(new UsuarioDTO2
                {
                    IdUsuario = item.IdUsuario,
                    NombreUsuario = item.NombreUsuario ?? string.Empty,
                    CorreoElectronico = item.CorreoElectronico ?? string.Empty,
                    Rol = item.Rol,
                    Confirmado = item.Confirmado
                });
            }

            return listaDTO;
        }
    }
    
}
