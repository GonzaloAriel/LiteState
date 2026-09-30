using System.ComponentModel.DataAnnotations;

namespace LiteState.Domain
{
    public class Dashboard
    {
        public int Id { get; set; }

        public int EmpresaId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        public int? SectorId { get; set; }

        public bool Activo { get; set; } = true;

        public Empresa Empresa { get; set; } = null!;

        public Sector? Sector { get; set; }
    }
}