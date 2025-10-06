using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using WorkAcademy.Models;
using WorkAcademy.Services;

namespace WorkAcademy.Controllers
{
    [Authorize(Roles = "Empresa")]
    public class EmpresasController : Controller
    {
        private readonly IEmpresaService _empresaService;

        public EmpresasController(IEmpresaService empresaService)
        {
            _empresaService = empresaService;
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Se já existir empresa para o usuário, manda direto criar vaga
            var todas = await _empresaService.ObterTodasEmpresas();
            var jaTem = todas.Any(e => e.IdentityUserId == userId);

            if (jaTem)
            {
                TempData["Sucesso"] = "Sua empresa já está cadastrada. Agora você pode criar vagas.";
                return RedirectToAction("Create", "Vagas");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Empresa empresa)
        {
            // Vincula com o usuário logado no SERVIDOR
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "Usuário não autenticado.");
            }
            else
            {
                empresa.IdentityUserId = userId;
            }

            // Impede cadastrar duplicado para o mesmo usuário
            var todas = await _empresaService.ObterTodasEmpresas();
            if (todas.Any(e => e.IdentityUserId == userId))
            {
                TempData["Sucesso"] = "Sua empresa já está cadastrada. Agora você pode criar vagas.";
                return RedirectToAction("Create", "Vagas");
            }

            if (ModelState.IsValid)
            {
                var result = await _empresaService.RegistrarEmpresa(empresa);
                if (result)
                {
                    TempData["Sucesso"] = "Empresa cadastrada com sucesso. Agora você pode criar vagas.";
                    return RedirectToAction("Create", "Vagas");
                }
                ModelState.AddModelError(string.Empty, "Não foi possível registrar a empresa. Tente novamente.");
            }

            return View(empresa);
        }

        // ===== EDIT =====

        [HttpGet]
        public async Task<IActionResult> Edit(System.Guid id)
        {
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(System.Guid id, Empresa empresa)
        {
            if (id != empresa.Id)
            {
                return NotFound();
            }

            // Garante que continue vinculado ao usuário logado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            empresa.IdentityUserId = userId;

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
