namespace LiteState.Domain
{
    public class UsuarioSector
    {
        public int UsuarioId { get; set; }

        public int SectorId { get; set; }

        public Usuario Usuario { get; set; } = null!;

        public Sector Sector { get; set; } = null!;
    }
}