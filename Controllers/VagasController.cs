using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkAcademy.Data;
using WorkAcademy.Models;

namespace WorkAcademy.Controllers
{
    [Authorize]
    public class VagasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VagasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vagas (público) -> somente aprovadas
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchString)
        {
            var q = _context.Vagas
                .AsNoTracking()
                .Include(v => v.Empresa)
                .Where(v => v.Aprovada) // apenas aprovadas
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var like = $"%{searchString.Trim()}%";
                q = q.Where(v =>
                       EF.Functions.Like(v.Nome ?? string.Empty, like)
                    || EF.Functions.Like(v.Descricao ?? string.Empty, like)
                    || EF.Functions.Like(v.Area ?? string.Empty, like)
                    || (v.Empresa != null && EF.Functions.Like(v.Empresa.NomeFantasia ?? string.Empty, like)));
            }

            ViewBag.CurrentFilter = searchString;
            var vagas = await q.OrderByDescending(v => v.DataPublicacao).ToListAsync();
            return View(vagas);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Empresa")]
        public IActionResult Create()
        {
            ViewBag.Empresas = new SelectList(_context.Empresas.OrderBy(e => e.NomeFantasia),
                                              "Id", "NomeFantasia");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Empresa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vaga model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Empresas = new SelectList(_context.Empresas.OrderBy(e => e.NomeFantasia),
                                                  "Id", "NomeFantasia", model.EmpresaId);
                return View(model);
            }

            model.DataPublicacao = DateTime.Now;
            _context.Vagas.Add(model);
            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Vaga publicada!";
            return RedirectToAction(nameof(Index));
        }

        // MINHAS VAGAS (Empresa) + badge de pendentes
        [Authorize(Roles = "Empresa")]
        public async Task<IActionResult> MinhasVagas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var empresa = await _context.Empresas
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdentityUserId == userId);

            if (empresa == null)
                return Unauthorized();

            var vagas = await _context.Vagas
                .AsNoTracking()
                .Where(v => v.EmpresaId == empresa.Id)
                .OrderByDescending(v => v.DataPublicacao)
                .ToListAsync();

            ViewBag.MinhasPendentes = vagas.Count(v => !v.Aprovada);
            return View(vagas);
        }

        // DETAILS (público) -> apenas aprovadas
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            var vaga = await _context.Vagas
                .AsNoTracking()
                .Include(v => v.Empresa)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vaga == null || !vaga.Aprovada)
                return NotFound();

            var outrasVagas = await _context.Vagas
                .AsNoTracking()
                .Where(v => v.EmpresaId == vaga.EmpresaId && v.Id != vaga.Id && v.Aprovada)
                .OrderByDescending(v => v.DataPublicacao)
                .Take(4)
                .ToListAsync();

            ViewBag.OutrasVagas = outrasVagas;
            return View(vaga);
        }

        // EDIT (GET): empresa só edita vaga dela e que NÃO foi aprovada
        [HttpGet]
        [Authorize(Roles = "Empresa")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.IdentityUserId == userId);
            if (empresa == null) return Unauthorized();

            var vaga = await _context.Vagas
                .Include(v => v.Empresa)
                .FirstOrDefaultAsync(v => v.Id == id && v.EmpresaId == empresa.Id);

            if (vaga == null) return NotFound();

            if (vaga.Aprovada)
            {
                TempData["Erro"] = "Esta vaga já foi aprovada e não pode mais ser editada.";
                return RedirectToAction(nameof(MinhasVagas));
            }

            return View(vaga);
        }

        // EDIT (POST): persiste alterações mantendo Aprovada=false
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Empresa")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Nome,Descricao,Area,Salario,Localizacao,TipoContrato,DataExpiracao")] Vaga form)
        {
            if (id != form.Id) return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.IdentityUserId == userId);
            if (empresa == null) return Unauthorized();

            var vaga = await _context.Vagas.FirstOrDefaultAsync(v => v.Id == id && v.EmpresaId == empresa.Id);
            if (vaga == null) return NotFound();

            if (vaga.Aprovada)
            {
                TempData["Erro"] = "Esta vaga já foi aprovada e não pode mais ser editada.";
                return RedirectToAction(nameof(MinhasVagas));
            }

            if (!ModelState.IsValid)
                return View(form);

            // Atualiza apenas campos editáveis pela empresa
            vaga.Nome = form.Nome?.Trim();
            vaga.Descricao = form.Descricao?.Trim();
            vaga.Area = form.Area?.Trim();
            vaga.Salario = form.Salario;
            vaga.Localizacao = form.Localizacao?.Trim();
            vaga.TipoContrato = form.TipoContrato?.Trim();
            vaga.DataExpiracao = form.DataExpiracao;

            // Mantém para revisão
            vaga.Aprovada = false;

            _context.Update(vaga);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Vaga atualizada e enviada novamente para análise do administrador.";
            return RedirectToAction(nameof(MinhasVagas));
        }
    }
}
