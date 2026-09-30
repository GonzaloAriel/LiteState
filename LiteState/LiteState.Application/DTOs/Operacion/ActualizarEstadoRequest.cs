namespace LiteState.Application.DTOs.Operacion
{
    public class ActualizarEstadoRequest
    {
        public int SectorId { get; set; }

        public int EstadoId { get; set; }

        public string? Campo1Valor { get; set; }

        public string? Campo2Valor { get; set; }

        public string? Campo3Valor { get; set; }
    }
}