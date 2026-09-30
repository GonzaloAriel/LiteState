using System.ComponentModel.DataAnnotations;

namespace LiteState.Domain
{
    public class Empresa
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        public bool Activa { get; set; } = true;

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<Sector> Sectores { get; set; } = new List<Sector>();
        public ICollection<Estado> Estados { get; set; } = new List<Estado>();
        public ICollection<Dashboard> Dashboards { get; set; } = new List<Dashboard>();
    }
}