using Microsoft.EntityFrameworkCore;
using SendEmail.Models;
using APIsEstudioJuridico.DTOs;
using SendEmail.DTOs;
namespace APIsEstudioJuridico.Services.Contrato
{
    public interface IUsuarioService
    {
        Task<Usuario?> GetUsuario(string correo, string clave);
        Task<Usuario> SaveUsuario(UsuarioDTO modelo);
        Task<Usuario> SaveUsuario2(UsuarioDTO modelo);
        Task<Usuario> RegistrarUsuario(UsuarioDTO modelo);
        Task<Usuario?> ObtenerUsuario(string correo);
        Task<Usuario> Obtener(string correo);
        Task<Usuario?> ObtenerUsuarioPorToken(string tokenRestablecimiento);
        Task<bool> Restablecer(int restablecer, string contrasenia, string tokenRestablecimiento);
        Task<bool> Actualizar(int restablecer, string contrasenia, string tokenRestablecimiento);
        Task<bool> Confirmar(int idUsuario);
        Task<bool> ActualizarUsuario(Usuario usuario);
        Task<bool> CambiarEstadoConfirmado(int id);
        Task<Usuario?> ObtenerUsuarioPorId(int idUsuario);
        Task<List<UsuarioDTO2>> ObtenerUsuariosConRolAbogado();
        Task<List<UsuarioDTO2>> ObtenerTodosLosUsuarios();
        Task<string?> ObtenerCorreoPorToken(string token);
        Task<bool> ConfirmarPorToken(string token);
        Task<bool> ConfirmarPorTokenConfirmado(string token);
        //Task<string> GenerarTokenRestablecimiento(int idUsuario);
    }
}
