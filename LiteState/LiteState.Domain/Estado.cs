using System.ComponentModel.DataAnnotations;

namespace LiteState.Domain
{
    public class Estado
    {
        public int Id { get; set; }

        public int EmpresaId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(7)]
        public string ColorHex { get; set; } = string.Empty;

        public int Orden { get; set; }

        public bool Activo { get; set; } = true;

        public bool EsFinalizado { get; set; }

        public Empresa Empresa { get; set; } = null!;
        public ICollection<Evento> Eventos { get; set; } = new List<Evento>();
        public ICollection<EstadoActual> EstadosActuales { get; set; } = new List<EstadoActual>();
    }
}