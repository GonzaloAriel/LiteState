namespace LiteState.Application.DTOs.Operacion
{
    public class OperacionViewModel
    {
        public string UsuarioNombre { get; set; } = string.Empty;

        public int SectorActualId { get; set; }

        public string SectorNombre { get; set; } = string.Empty;

        public List<SectorItemViewModel> Sectores { get; set; } = new();

        public string Campo1Nombre { get; set; } = "Orden";

        public string Campo2Nombre { get; set; } = "Lote";

        public string Campo3Nombre { get; set; } = "Cantidad";

        public string? Campo1Valor { get; set; }

        public string? Campo2Valor { get; set; }

        public string? Campo3Valor { get; set; }

        public List<EstadoItemViewModel> Estados { get; set; } = new();
    }
}