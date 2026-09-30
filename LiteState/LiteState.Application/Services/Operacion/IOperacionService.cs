using LiteState.Application.DTOs.Operacion;

namespace LiteState.Application.Services.Operacion
{
    public interface IOperacionService
    {
        OperacionViewModel ObtenerPantalla(int? sectorId = null);

        Task<bool> ActualizarEstadoAsync(ActualizarEstadoRequest request);

        bool CambiarSector(int sectorId);
    }
}