using Microsoft.AspNetCore.Mvc;
using WorkAcademy.Services;

namespace WorkAcademy.Controllers
{
    public class HomeController : Controller
    {
        private readonly IVagaService _vagaService;

        public HomeController(IVagaService vagaService)
        {
            _vagaService = vagaService;
        }

        public async Task<IActionResult> Index()
        {
            var vagas = await _vagaService.ObterTodasVagas();
            return View(vagas);
        }

        public IActionResult Sobre()
        {
            return View();
        }

        public IActionResult Contato()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Termos()
        {
            return View("~/Views/Shared/TermosDeUso.cshtml");
        }
    }
}