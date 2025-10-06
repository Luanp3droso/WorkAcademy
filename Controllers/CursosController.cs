using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WorkAcademy.Data;
using WorkAcademy.Models;
using System.Security.Claims;
using WorkAcademy.Services.Notifications;   

namespace WorkAcademy.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Cursos
        public async Task<IActionResult> Index(string busca, string categoria, string nivel, int? duracaoMin, int? duracaoMax)
        {
            var cursosQuery = _context.Cursos.AsQueryable();

            if (!string.IsNullOrEmpty(busca))
            {
                cursosQuery = cursosQuery.Where(c =>
                    c.Nome.Contains(busca) || c.NomeInstrutor.Contains(busca));
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                cursosQuery = cursosQuery.Where(c => c.Categoria == categoria);
            }

            if (!string.IsNullOrEmpty(nivel))
            {
                cursosQuery = cursosQuery.Where(c => c.Nivel == nivel);
            }

            if (duracaoMin.HasValue)
            {
                cursosQuery = cursosQuery.Where(c => c.DuracaoMeses >= duracaoMin);
            }

            if (duracaoMax.HasValue)
            {
                cursosQuery = cursosQuery.Where(c => c.DuracaoMeses <= duracaoMax);
            }

            var categorias = await _context.Cursos.Select(c => c.Categoria).Distinct().ToListAsync();
            var niveis = await _context.Cursos.Select(c => c.Nivel).Distinct().ToListAsync();

            ViewBag.Categorias = categorias;
            ViewBag.Niveis = niveis;

            var cursos = await cursosQuery.ToListAsync();
            return View(cursos);
        }

        // GET: /Cursos/Detalhes/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);
            if (curso == null)
                return NotFound();

            return View("Details", curso); // Importante: indicar que a View se chama "Details"
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Desativar(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso != null)
            {
                curso.Ativo = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Conteudos", "Admin");
        }

        // POST: /Cursos/Inscrever
        [Authorize]
        public async Task<IActionResult> Inscrever(Guid id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
                return NotFound();

            var userIdentityId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdentityUserId == userIdentityId);

            if (usuario == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction("Index");
            }

            // Verifica se já está inscrito
            var jaInscrito = await _context.CursosUsuarios
                .AnyAsync(cu => cu.CursoId == id && cu.UsuarioId == usuario.Id);

            if (!jaInscrito)
            {
                var inscricao = new CursoUsuario
                {
                    CursoId = id,
                    UsuarioId = usuario.Id,
                    UsuarioIdentityId = usuario.IdentityUserId,
                    DataInscricao = DateTime.Now,
                    Concluido = false
                };

                _context.CursosUsuarios.Add(inscricao);
                await _context.SaveChangesAsync();
            }

            TempData["Mensagem"] = "Inscrição realizada com sucesso!";
            return RedirectToAction("Index");
        }
        // GET: /Cursos/Create
        [HttpGet]
        [Authorize(Roles = "Admin,Empresa")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Cursos/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Empresa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Curso curso)
        {
            if (ModelState.IsValid)
            {
                curso.Id = Guid.NewGuid();
                _context.Cursos.Add(curso);
                await _context.SaveChangesAsync();

                TempData["Mensagem"] = "Curso criado com sucesso!";
                return RedirectToAction("Index");
            }

            return View(curso);
        }
    }
}
