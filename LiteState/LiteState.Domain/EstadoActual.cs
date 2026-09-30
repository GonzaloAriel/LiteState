using System.ComponentModel.DataAnnotations;

namespace LiteState.Domain
{
    public class EstadoActual
    {
        public int SectorId { get; set; }

        public int EstadoId { get; set; }

        public int UsuarioId { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string? Campo1Valor { get; set; }

        [MaxLength(200)]
        public string? Campo2Valor { get; set; }

        [MaxLength(200)]
        public string? Campo3Valor { get; set; }

        public Sector Sector { get; set; } = null!;

        public Estado Estado { get; set; } = null!;

        public Usuario Usuario { get; set; } = null!;
    }
}