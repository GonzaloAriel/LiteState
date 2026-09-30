using LiteState.Application.DTOs.Operacion;
using LiteState.Application.Services.Operacion;
using Microsoft.AspNetCore.Mvc;

namespace LiteState.Controllers
{
    public class OperacionController : Controller
    {
        private readonly IOperacionService _operacionService;

        public OperacionController(IOperacionService operacionService)
        {
            _operacionService = operacionService;
        }

        // -------------------------
        // VIEW PRINCIPAL
        // -------------------------
        [HttpGet]
        public IActionResult Index()
        {
            var model = _operacionService.ObtenerPantalla();
            return View(model);
        }

        // -------------------------
        // ACTUALIZAR ESTADO (AJAX)
        // -------------------------
        [HttpPost]
        public async Task<IActionResult> ActualizarEstado([FromBody] ActualizarEstadoRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    ok = false,
                    message = "Request inválido"
                });
            }

            var result = await _operacionService.ActualizarEstadoAsync(request);

            return Json(new
            {
                ok = result,
                message = result ? "Estado actualizado" : "Error al actualizar estado"
            });
        }

        // -------------------------
        // CAMBIAR SECTOR (AJAX)
        // -------------------------
        [HttpPost]
        public IActionResult CambiarSector([FromBody] CambiarSectorRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    ok = false,
                    message = "Request inválido"
                });
            }

            var ok = _operacionService.CambiarSector(request.SectorId);

            if (!ok)
            {
                return BadRequest(new
                {
                    ok = false,
                    message = "No se pudo cambiar el sector"
                });
            }

            return Json(new
            {
                ok = true,
                message = "Sector cambiado correctamente"
            });
        }

        // -------------------------
        // PANEL PARCIAL (REFRESH SPA LIGERO)
        // -------------------------
        [HttpGet]
        public IActionResult ObtenerPanelSector(int sectorId)
        {
            var model = _operacionService.ObtenerPantalla(sectorId);

            return PartialView("_OperacionPanel", model);
        }
    }
}