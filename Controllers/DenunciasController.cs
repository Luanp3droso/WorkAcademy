using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkAcademy.Data;
using WorkAcademy.Models;

namespace WorkAcademy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DenunciasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DenunciasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var denuncias = await _context.Denuncias
                .OrderByDescending(d => d.Data)
                .ToListAsync();
            return View(denuncias);
        }

        [HttpPost]
        public async Task<IActionResult> MarcarComoResolvida(int id)
        {
            var denuncia = await _context.Denuncias.FindAsync(id);
            if (denuncia != null)
            {
                denuncia.Resolvido = true;
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Denúncia marcada como resolvida.";
            }

            return RedirectToAction("Index");
        }
    }
}
