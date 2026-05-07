using Microsoft.AspNetCore.Mvc;

namespace LiteState.Controllers
{
    public class StatusController : Controller
    {
        public IActionResult Index()
        {
            var result = new
            {
                app = "LiteState",
                status = "OK",
                serverTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                machine = Environment.MachineName
            };

            return Json(result);
        }
    }
}