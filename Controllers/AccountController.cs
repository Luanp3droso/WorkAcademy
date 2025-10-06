using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkAcademy.Models;
using WorkAcademy.Data;
using System.Security.Claims;

namespace WorkAcademy.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager,
                                 RoleManager<IdentityRole> roleManager,
                                 ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet("Login")]
        public IActionResult Login() => View();

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string senha)
        {
            var identityUser = await _userManager.FindByEmailAsync(email);
            if (identityUser != null)
            {
                if (identityUser.LockoutEnabled && identityUser.LockoutEnd.HasValue && identityUser.LockoutEnd > DateTimeOffset.UtcNow)
                {
                    ViewBag.Erro = "Este usuário está bloqueado.";
                    return View("Login");
                }
                var result = await _signInManager.PasswordSignInAsync(identityUser, senha, false, false);
                if (result.Succeeded)
                {
                    if (await _userManager.IsInRoleAsync(identityUser, "Admin"))
                        return RedirectToAction("Painel", "Admin");

                    return RedirectToAction("Home", "Usuario");
                }
            }

            ViewBag.Erro = "Email ou senha inválidos.";
            return View("Login");
        }

        [HttpGet("Register")]
        public IActionResult Register() => View();

        [HttpPost("Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Usuario model)
        {
            if (Request.Form["aceitaTermos"] != "on")
                ModelState.AddModelError("termos", "Você deve aceitar os Termos de Uso para continuar.");

            if (!ModelState.IsValid)
            {
                ViewBag.Erro = "Preencha todos os campos obrigatórios.";
                return View("Register", model);
            }

            try
            {
                var identityUser = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(identityUser, model.Senha);

                if (result.Succeeded)
                {
                    // Garante que o papel "User" exista
                    if (!await _roleManager.RoleExistsAsync("User"))
                        await _roleManager.CreateAsync(new IdentityRole("User"));

                    await _userManager.AddToRoleAsync(identityUser, "User");

                    model.IdentityUserId = identityUser.Id;
                    model.DataCadastro = DateTime.Now;
                    _context.Usuarios.Add(model);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(identityUser, isPersistent: false);
                    TempData["Sucesso"] = "Usuário cadastrado com sucesso!";
                    return RedirectToAction("Home", "Usuario");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View("Register", model);
            }
            catch (Exception ex)
            {
                ViewBag.Erro = $"Erro ao salvar no banco: {ex.Message}";
                return View("Register", model);
            }
        }

        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpPost("ExternalLogin")]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                ViewBag.Erro = $"Erro do provedor externo: {remoteError}";
                return RedirectToAction("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction("Login");

            var email = info.Principal?.FindFirstValue(ClaimTypes.Email) ?? $"user{Guid.NewGuid()}@workacademy.com";
            var nome = info.Principal?.FindFirstValue(ClaimTypes.Name) ?? "Usuário Social";

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
                return RedirectToAction("Home", "Usuario");

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                existingUser = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var createResult = await _userManager.CreateAsync(existingUser);
                if (!createResult.Succeeded)
                {
                    ViewBag.Erro = "Erro ao criar usuário externo.";
                    return RedirectToAction("Login");
                }

                await _userManager.AddLoginAsync(existingUser, info);

                // Cria também o perfil na tabela Usuario
                var usuario = new Usuario
                {
                    NomeCompleto = nome,
                    Email = email,
                    CPF = "000.000.000-00",
                    Senha = "SocialLogin",
                    Endereco = "-",
                    Telefone = "-",
                    Celular = "-",
                    AreaInteresse = "Não informado",
                    IdentityUserId = existingUser.Id,
                    DataCadastro = DateTime.Now
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                if (!await _roleManager.RoleExistsAsync("User"))
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                await _userManager.AddToRoleAsync(existingUser, "User");
            }

            await _signInManager.SignInAsync(existingUser, isPersistent: false);
            return RedirectToAction("Home", "Usuario");
        }
    }
}
