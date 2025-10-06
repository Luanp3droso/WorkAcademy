
using Microsoft.AspNetCore.Mvc;

namespace WorkAcademy.Controllers
{
    [Route("Empresa")]
    public class EmpresaController : Controller
    {
        [HttpGet("Perfil")]
        public IActionResult Perfil()
        {
            return View();
        }
    }
}
