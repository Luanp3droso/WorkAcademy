using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkAcademy.Data;
using WorkAcademy.Models;
using WorkAcademy.Models.ViewModels;

namespace WorkAcademy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context,
                               UserManager<IdentityUser> userManager,
                               RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Painel principal
        public IActionResult Index()
        {
            return View("Painel");
        }

        public async Task<IActionResult> Conteudos()
        {
            var cursos = await _context.Cursos
                .Include(c => c.Empresa)
                .ToListAsync();

            var vagas = await _context.Vagas
                .Include(v => v.Empresa)
                .ToListAsync();

            var publicacoes = await _context.Publicacoes
                .Include(p => p.Usuario)
                .ToListAsync();

            var model = new ConteudosViewModel
            {
                Cursos = cursos,
                Vagas = vagas,
                Publicacoes = publicacoes
            };

            return View(model);
        }

        // Lista de usuários com roles e status de bloqueio
        public async Task<IActionResult> Usuarios(string filtroEmail, string filtroRole, string filtroStatus)
        {
            var usuarios = await _userManager.Users.ToListAsync();
            var lista = new List<UsuarioComRolesViewModel>();

            foreach (var user in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var bloqueado = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;

                lista.Add(new UsuarioComRolesViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    Bloqueado = bloqueado
                });
            }

            // Aplicar filtros
            if (!string.IsNullOrEmpty(filtroEmail))
                lista = lista.Where(u => u.Email != null && u.Email.ToLower().Contains(filtroEmail.ToLower())).ToList();

            if (!string.IsNullOrEmpty(filtroRole) && filtroRole != "Todos")
                lista = lista.Where(u => u.Roles.Contains(filtroRole)).ToList();

            if (!string.IsNullOrEmpty(filtroStatus) && filtroStatus != "Todos")
            {
                if (filtroStatus == "Ativo")
                    lista = lista.Where(u => !u.Bloqueado).ToList();
                else if (filtroStatus == "Bloqueado")
                    lista = lista.Where(u => u.Bloqueado).ToList();
            }

            return View(lista);
        }

        // Atribui role ao usuário (remove anterior antes)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtribuirRole(string userId, string role)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario != null && (role == "Empresa" || role == "Admin" || role == "User"))
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

                var rolesAtuais = await _userManager.GetRolesAsync(usuario);
                await _userManager.RemoveFromRolesAsync(usuario, rolesAtuais);
                await _userManager.AddToRoleAsync(usuario, role);

                TempData["Sucesso"] = $"Usuário promovido para {role}.";
            }
            else
            {
                TempData["Erro"] = "Usuário ou role inválida.";
            }

            return RedirectToAction("Usuarios");
        }

        // Bloqueia usuário
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BloquearUsuario(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
                await _userManager.UpdateAsync(user);
                TempData["Sucesso"] = "Usuário bloqueado com sucesso.";
            }

            return RedirectToAction("Usuarios");
        }

        // Desbloqueia usuário
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesbloquearUsuario(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.LockoutEnd = null;
                await _userManager.UpdateAsync(user);
                TempData["Sucesso"] = "Usuário desbloqueado com sucesso.";
            }

            return RedirectToAction("Usuarios");
        }

        public async Task<IActionResult> Painel()
        {
            var totalDenunciasPendentes = await _context.Denuncias.CountAsync(d => !d.Resolvido);
            ViewBag.DenunciasPendentes = totalDenunciasPendentes;
            return View();
        }

        // ====== MODERAÇÃO DE VAGAS ======

        // Lista vagas pendentes (não aprovadas)
        public async Task<IActionResult> VagasPendentes()
        {
            var vagas = await _context.Vagas
                .Include(v => v.Empresa)
                .Where(v => !v.Aprovada)
                .OrderByDescending(v => v.DataPublicacao)
                .ToListAsync();

            return View(vagas);
        }

        // Aprovar vaga
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarVaga(Guid id)
        {
            var vaga = await _context.Vagas.FindAsync(id);
            if (vaga == null) return NotFound();

            vaga.Aprovada = true;
            vaga.MotivoRejeicao = null; // limpa motivo anterior se houver
            vaga.DataPublicacao = DateTime.Now;

            _context.Update(vaga);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Vaga aprovada com sucesso.";
            return RedirectToAction(nameof(VagasPendentes));
        }

        // Rejeitar vaga (permanece para edição da empresa) + motivo obrigatório
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarVaga(Guid id, string? motivo)
        {
            var vaga = await _context.Vagas.FindAsync(id);
            if (vaga == null) return NotFound();

            if (string.IsNullOrWhiteSpace(motivo))
            {
                TempData["Erro"] = "Informe um motivo para rejeição.";
                return RedirectToAction(nameof(VagasPendentes));
            }

            vaga.Aprovada = false;                 // garante pendente
            vaga.MotivoRejeicao = motivo.Trim();   // feedback visível para a empresa

            _context.Update(vaga);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Vaga rejeitada. A empresa poderá editar e reenviar.";
            return RedirectToAction(nameof(VagasPendentes));
        }
        // ====== FIM MODERAÇÃO DE VAGAS ======
    }

    public class UsuarioComRolesViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool Bloqueado { get; set; }
    }
}
