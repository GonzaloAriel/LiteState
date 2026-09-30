using System.ComponentModel.DataAnnotations;

namespace LiteState.Domain
{
    public class Sector
    {
        public int Id { get; set; }

        public int EmpresaId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        public int Orden { get; set; }

        public bool Activo { get; set; } = true;

        [MaxLength(100)]
        public string? Campo1Nombre { get; set; }

        [MaxLength(50)]
        public string? Campo1Tipo { get; set; }

        [MaxLength(100)]
        public string? Campo2Nombre { get; set; }

        [MaxLength(50)]
        public string? Campo2Tipo { get; set; }

        [MaxLength(100)]
        public string? Campo3Nombre { get; set; }

        [MaxLength(50)]
        public string? Campo3Tipo { get; set; }

        public Empresa Empresa { get; set; } = null!;
        public ICollection<UsuarioSector> UsuarioSectores { get; set; } = new List<UsuarioSector>();
        public ICollection<Evento> Eventos { get; set; } = new List<Evento>();
        public EstadoActual? EstadoActual { get; set; }
        public ICollection<Dashboard> Dashboards { get; set; } = new List<Dashboard>();
    }
}