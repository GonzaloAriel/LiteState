using LiteState.Application.DTOs.Operacion;

namespace LiteState.Application.Services.Operacion
{
    public class OperacionService : IOperacionService
    {
        public OperacionViewModel ObtenerPantalla(int? sectorId = null)
        {
            var sectores = new List<SectorItemViewModel>
            {
                new() { Id = 1, Nombre = "Producción Línea 1" },
                new() { Id = 2, Nombre = "Empaque" },
                new() { Id = 3, Nombre = "Depósito" }
            };

            var sectorActual = sectores
                .FirstOrDefault(x => x.Id == sectorId)
                ?? sectores.First();

            return new OperacionViewModel
            {
                UsuarioNombre = "Juan Pérez",

                SectorActualId = sectorActual.Id,

                SectorNombre = sectorActual.Nombre,

                Sectores = sectores,

                Campo1Nombre = "Orden",
                Campo2Nombre = "Lote",
                Campo3Nombre = "Cantidad",

                Estados = new List<EstadoItemViewModel>
                {
                    new()
                    {
                        Id = 1,
                        Nombre = "🟢 PRODUCIENDO",
                        Tone = "success"
                    },

                    new()
                    {
                        Id = 2,
                        Nombre = "🟡 ESPERANDO",
                        Tone = "warning"
                    },

                    new()
                    {
                        Id = 3,
                        Nombre = "🔴 PROBLEMA",
                        Tone = "danger"
                    },

                    new()
                    {
                        Id = 4,
                        Nombre = "⚫ FINALIZADO",
                        Tone = "neutral"
                    }
                }
            };
        }

        public async Task<bool> ActualizarEstadoAsync(ActualizarEstadoRequest request)
        {
            // 1. Simulación lógica (por ahora sin DB)
            Console.WriteLine("=== ESTADO ACTUALIZADO ===");
            Console.WriteLine($"Sector: {request.SectorId}");
            Console.WriteLine($"Estado: {request.EstadoId}");

            // 2. Aquí luego irá EF Core:
            // - Insert Evento
            // - Update EstadoActual

            return true;
        }

        public bool CambiarSector(int sectorId)
        {
            // acá después vas a usar DB o sesión

            Console.WriteLine($"Cambiando sector a: {sectorId}");

            return true;
        }
    }
}