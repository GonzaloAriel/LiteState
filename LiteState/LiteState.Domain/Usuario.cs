using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LiteState.Domain
{
    public class Usuario : IdentityUser<int>
    {
        [Required]
        [MaxLength(200)]
        public string NombreCompleto { get; set; } = string.Empty;

        public int EmpresaId { get; set; }

        public bool Activo { get; set; } = true;

        public Empresa Empresa { get; set; } = null!;
        public ICollection<UsuarioSector> UsuarioSectores { get; set; } = new List<UsuarioSector>();
        public ICollection<Evento> Eventos { get; set; } = new List<Evento>();
    }
}