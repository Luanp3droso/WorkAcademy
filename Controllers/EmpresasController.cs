using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using WorkAcademy.Models;
using WorkAcademy.Services;

namespace WorkAcademy.Controllers
{
    [Authorize]
    public class EmpresasController : Controller
    {
        private readonly IEmpresaService _empresaService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public EmpresasController(
            IEmpresaService empresaService,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager)
        {
            _empresaService = empresaService;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // Lista (opcional deixar só para Admin depois)
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var empresas = await _empresaService.ObterTodasEmpresas();
            return View(empresas);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(System.Guid id)
        {
            var empresa = await _empresaService.ObterEmpresaPorId(id);
            if (empresa == null)
            {
                return NotFound();
            }
            return View(empresa);
        }

        // ===== CREATE =====

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ModelState.Remove(nameof(Empresa.IdentityUserId));
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // Se já existir empresa para o usuário, manda direto criar vaga
            var empresaExistente = await _empresaService.ObterEmpresaPorUsuario(userId);
            var jaTem = empresaExistente != null;

            if (jaTem)
            {
                TempData["Sucesso"] = "Sua empresa já está cadastrada. Agora você pode criar vagas.";
                return RedirectToAction("Create", "Vagas");
            }

            return View(new Empresa
            {
                IdentityUserId = userId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Empresa empresa)
        {
            // Remove qualquer erro automático do IdentityUserId e vincula ao usuário logado
            ModelState.Remove(nameof(Empresa.IdentityUserId));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            else
            {
                empresa.IdentityUserId = userId;
                // Remove valores anteriores do ModelState e revalida com o usuário atual
                ModelState.Clear();
                TryValidateModel(empresa);
            }

            // Impede cadastrar duplicado para o mesmo usuário
            var empresaExistente = await _empresaService.ObterEmpresaPorUsuario(userId);
            if (empresaExistente != null)
            {
                TempData["Sucesso"] = "Sua empresa já está cadastrada. Agora você pode criar vagas.";
                return RedirectToAction("Create", "Vagas");
            }

            if (ModelState.IsValid)
            {
                var result = await _empresaService.RegistrarEmpresa(empresa);
                if (result)
                {
                    // Garante que o usuário receba o papel "Empresa" após o cadastro
                    var identityUser = await _userManager.FindByIdAsync(userId);
                    if (identityUser != null)
                    {
                        if (!await _roleManager.RoleExistsAsync("Empresa"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole("Empresa"));
                        }

                        if (!await _userManager.IsInRoleAsync(identityUser, "Empresa"))
                        {
                            await _userManager.AddToRoleAsync(identityUser, "Empresa");
                            await _signInManager.RefreshSignInAsync(identityUser);
                        }
                    }

                    TempData["Sucesso"] = "Empresa cadastrada com sucesso. Agora você pode criar vagas.";
                    return RedirectToAction("Create", "Vagas");
                }
                ModelState.AddModelError(string.Empty, "Não foi possível registrar a empresa. Tente novamente.");
            }

            return View(empresa);
        }

        // ===== EDIT =====

        [Authorize(Roles = "Empresa,Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(System.Guid id)
        {
            ModelState.Remove(nameof(Empresa.IdentityUserId));
            var empresa = await _empresaService.ObterEmpresaPorId(id);
            if (empresa == null)
            {
                return NotFound();
            }

            // (Opcional) validar se a empresa pertence ao usuário logado
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (empresa.IdentityUserId != userId) return Forbid();

            return View(empresa);
        }

        [Authorize(Roles = "Empresa,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(System.Guid id, Empresa empresa)
        {
            if (id != empresa.Id)
            {
                return NotFound();
            }

            // Garante que continue vinculado ao usuário logado
            ModelState.Remove(nameof(Empresa.IdentityUserId));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "Usuário não autenticado.");
            }
            else
            {
                empresa.IdentityUserId = userId;
            }

            if (ModelState.IsValid)
            {
                var result = await _empresaService.AtualizarEmpresa(empresa);
                if (result)
                {
                    TempData["Sucesso"] = "Empresa atualizada com sucesso.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Não foi possível atualizar a empresa.");
            }
            return View(empresa);
        }

        // ===== DELETE =====

        [Authorize(Roles = "Empresa,Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(System.Guid id)
        {
            var empresa = await _empresaService.ObterEmpresaPorId(id);
            if (empresa == null)
            {
                return NotFound();
            }
            return View(empresa);
        }

        [Authorize(Roles = "Empresa,Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(System.Guid id)
        {
            var result = await _empresaService.RemoverEmpresa(id);
            if (result)
            {
                TempData["Sucesso"] = "Empresa removida com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            return Problem("Erro ao excluir a empresa");
        }
    }
}