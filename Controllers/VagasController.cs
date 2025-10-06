using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkAcademy.Data;
using WorkAcademy.Models;
using WorkAcademy.Services.Notifications;

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
        public async Task<IActionResult> Index(string searchString)
        {
            var vagas = _context.Vagas
                .Include(v => v.Empresa)
                .Where(v => v.Aprovada) // Apenas vagas aprovadas
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                vagas = vagas.Where(v =>
                    v.Nome.Contains(searchString) ||
                    v.Descricao.Contains(searchString) ||
                    v.Empresa.NomeFantasia.Contains(searchString) ||
                    v.Area.Contains(searchString));
            }

            return View(await vagas.OrderByDescending(v => v.DataPublicacao).ToListAsync());
        }

        // GET: Vagas/Create
        [HttpGet]
        [Authorize(Roles = "Empresa,Admin")]
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.Empresas = _context.Empresas
                    .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.NomeFantasia })
                    .ToList();
                return View("Create");
            }

            // EMPRESA: exige empresa vinculada ao usuário
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.IdentityUserId == userId);

            if (empresa == null)
            {
                TempData["Erro"] = "Você precisa completar o cadastro da sua empresa antes de criar vagas.";
                return RedirectToAction("Create", "Empresas"); // requer Views/Empresas/Create.cshtml
            }

            return View("Create");
        }

        // POST: Vagas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Empresa,Admin")]
        public async Task<IActionResult> Create([Bind("Nome,Descricao,EmpresaId,Area,Salario,Localizacao,TipoContrato,DataExpiracao")] Vaga vaga)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Empresa? empresa = null;

            if (User.IsInRole("Empresa"))
            {
                // Defesa: impede criar vaga sem ter Empresa vinculada
                empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.IdentityUserId == userId);
                if (empresa == null)
                {
                    TempData["Erro"] = "Você precisa completar o cadastro da sua empresa antes de criar vagas.";
                    return RedirectToAction("Create", "Empresas");
                }
            }
            else if (User.IsInRole("Admin"))
            {
                if (vaga.EmpresaId == Guid.Empty || !_context.Empresas.Any(e => e.Id == vaga.EmpresaId))
                {
                    ModelState.AddModelError("EmpresaId", "Selecione uma empresa válida.");
                }
                else
                {
                    empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == vaga.EmpresaId);
                }
            }

            if (ModelState.IsValid && empresa != null)
            {
                vaga.Id = Guid.NewGuid();
                vaga.EmpresaId = empresa.Id;
                vaga.DataPublicacao = DateTime.Now;
                vaga.Aprovada = false;          // vai para aprovação
                // vaga.MotivoRejeicao = null;  // se existir, você pode limpar aqui

                _context.Vagas.Add(vaga);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Vaga criada com sucesso e enviada para aprovação!";
                return RedirectToAction(nameof(Index));
            }

            // Recarregar dropdown se Admin (em caso de erro de validação)
            if (User.IsInRole("Admin"))
            {
                ViewBag.Empresas = _context.Empresas
                    .Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = e.NomeFantasia
                    })
                    .ToList();
            }

            TempData["Erro"] = "Erro ao criar vaga. Verifique os campos.";
            return View("Create", vaga);
        }

        // MINHAS VAGAS (Empresa) + badge de pendentes
        [Authorize(Roles = "Empresa")]
        public async Task<IActionResult> MinhasVagas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.IdentityUserId == userId);

            if (empresa == null)
                return Unauthorized();

            var vagas = await _context.Vagas
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
                .Include(v => v.Empresa)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vaga == null || !vaga.Aprovada)
                return NotFound();

            var outrasVagas = await _context.Vagas
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
            {
                return View(form);
            }

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
            // Opcional: resetar motivo quando reenvia
            // vaga.MotivoRejeicao = null;
            // Opcional: marcar data de reenvio
            // vaga.DataPublicacao = DateTime.Now;

            _context.Update(vaga);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Vaga atualizada e enviada novamente para análise do administrador.";
            return RedirectToAction(nameof(MinhasVagas));
        }
    }
}
