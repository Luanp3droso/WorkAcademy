// Controllers/PublicaDenunciaController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkAcademy.Data;
using WorkAcademy.Models;
using System.Security.Claims;

namespace WorkAcademy.Controllers
{
    [Authorize]
    public class PublicaDenunciaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicaDenunciaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Criar(string tipo, int id)
        {
            ViewBag.Tipo = tipo;
            ViewBag.ConteudoId = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(Denuncia model)
        {
            if (string.IsNullOrEmpty(model.Motivo))
            {
                ModelState.AddModelError("Motivo", "Informe o motivo da denúncia.");
                return View(model);
            }

            model.UsuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.Data = DateTime.Now;
            model.Resolvido = false;

            _context.Denuncias.Add(model);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Denúncia enviada com sucesso!";
            return RedirectToAction("Index", "Home");
        }
    }
}
