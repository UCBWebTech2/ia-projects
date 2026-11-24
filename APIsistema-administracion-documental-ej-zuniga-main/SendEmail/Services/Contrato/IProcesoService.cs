using APIsEstudioJuridico.DTOs;
using SendEmail.DTOs;
using SendEmail.Models;

namespace APIsEstudioJuridico.Services.Contrato
{
    public interface IProcesoService
    {
        Task<List<ProcesoDTO>> Lista();
        Task<ProcesoDTO?> ObtenerPorId(int id);
        Task CrearProcesoAsync(ProcesoDTO procesoDTO);
        Task<bool> ActualizarProceso(int id, ProcesoDTO procesoDTO);
        Task<bool> EliminarProceso(int id);
        Task<bool> CambiarEstado(int id);
        Task<List<ProcesoDTO>> ListaHabilitados();
    }
}
