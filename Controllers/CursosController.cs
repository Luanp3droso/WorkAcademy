using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using WorkAcademy.Data;
using WorkAcademy.Models;
using System.Security.Claims;
using System.IO;

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
        public async Task<IActionResult> Index(string? busca, string? categoria, string? nivel, int? duracaoMin, int? duracaoMax)
        {
            var cursosQuery = _context.Cursos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var like = $"%{busca.Trim()}%";
                cursosQuery = cursosQuery.Where(c =>
                    EF.Functions.Like(c.Nome, like) ||
                    EF.Functions.Like(c.NomeInstrutor, like));
            }

            if (!string.IsNullOrWhiteSpace(categoria))
                cursosQuery = cursosQuery.Where(c => c.Categoria == categoria);

            if (!string.IsNullOrWhiteSpace(nivel))
                cursosQuery = cursosQuery.Where(c => c.Nivel == nivel);

            if (duracaoMin.HasValue)
                cursosQuery = cursosQuery.Where(c => c.DuracaoMeses >= duracaoMin);

            if (duracaoMax.HasValue)
                cursosQuery = cursosQuery.Where(c => c.DuracaoMeses <= duracaoMax);

            var categorias = await _context.Cursos
                .Select(c => c.Categoria)
                .Where(c => c != null && c != "")
                .Distinct()
                .ToListAsync();

            var niveis = await _context.Cursos
                .Select(c => c.Nivel)
                .Where(n => n != null && n != "")
                .Distinct()
                .ToListAsync();

            ViewBag.Categorias = categorias;
            ViewBag.Niveis = niveis;

            var cursos = await cursosQuery.ToListAsync();
            return View(cursos);
        }

        // GET: /Cursos/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);
            if (curso == null)
                return NotFound();

            return View("Details", curso);
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
        public async Task<IActionResult> Create(Curso curso, IFormFile? imagemArquivo)
        {
            if (!ModelState.IsValid)
                return View(curso);

            // Se veio arquivo, salva em wwwroot/img/cursos e define ImagemUrl
            if (imagemArquivo is not null && imagemArquivo.Length > 0)
            {
                // validação simples de mime
                var contentTypeOk =
                    imagemArquivo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
                if (!contentTypeOk)
                {
                    ModelState.AddModelError(nameof(curso.ImagemUrl), "Envie um arquivo de imagem válido.");
                    return View(curso);
                }

                var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var pasta = Path.Combine(wwwRoot, "img", "cursos");
                if (!Directory.Exists(pasta))
                    Directory.CreateDirectory(pasta);

                var nomeArquivo = $"curso_{DateTime.UtcNow.Ticks}{Path.GetExtension(imagemArquivo.FileName)}";
                var caminho = Path.Combine(pasta, nomeArquivo);

                using (var fs = new FileStream(caminho, FileMode.Create))
                    await imagemArquivo.CopyToAsync(fs);

                // Define a URL pública
                curso.ImagemUrl = $"/img/cursos/{nomeArquivo}";
            }

            // Garante um Id
            if (curso.Id == Guid.Empty)
                curso.Id = Guid.NewGuid();

            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();

            TempData["Mensagem"] = "Curso criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
