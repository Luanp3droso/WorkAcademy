using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkAcademy.Data;
using WorkAcademy.Models;

namespace WorkAcademy.Controllers
{
    public class CursoUsuarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursoUsuarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Inscrever(Guid id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
                return NotFound();

            var userIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var jaInscrito = await _context.CursosUsuarios
                .AnyAsync(cu => cu.CursoId == id && cu.UsuarioIdentityId == userIdentityId);

            if (!jaInscrito)
            {
                var inscricao = new CursoUsuario
                {
                    CursoId = id,
                    DataInscricao = DateTime.Now,
                    Concluido = false,
                    UsuarioIdentityId = userIdentityId
                };

                _context.CursosUsuarios.Add(inscricao);
                await _context.SaveChangesAsync();
            }

            TempData["Mensagem"] = "Inscrição realizada com sucesso!";
            return RedirectToAction("Index");
        }

    }
}
