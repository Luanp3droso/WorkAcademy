using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkAcademy.Models;
using WorkAcademy.Data;
using System.Security.Claims;
using System.Net.Mail;
using System.Net;
using System.Linq;

namespace WorkAcademy.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AccountController(UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager,
                                 RoleManager<IdentityRole> roleManager,
                                 ApplicationDbContext context,
                                 IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _config = config;
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
 
                }
            }

            ViewBag.Erro = "Email ou senha inválidos.";
            return View("Login");
        }
        [HttpGet("ForgotPassword")]
        public IActionResult ForgotPassword() => View();

        [HttpPost("ForgotPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email, string celular, string novaSenha, string confirmarSenha)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(celular) || string.IsNullOrWhiteSpace(novaSenha) || string.IsNullOrWhiteSpace(confirmarSenha))
            {
                ViewBag.Erro = "Preencha todos os campos para recuperar a senha.";
                return View();
            }

            if (novaSenha != confirmarSenha)
            {
                ViewBag.Erro = "As senhas não coincidem.";
                return View();
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email && u.Celular == celular);
            if (usuario == null)
            {
                ViewBag.Erro = "Não foi possível localizar um usuário com o e-mail e celular informados.";
                return View();
            }

            var identityUser = await _userManager.FindByEmailAsync(email);
            if (identityUser == null)
            {
                ViewBag.Erro = "Usuário não encontrado no sistema de autenticação.";
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
            var result = await _userManager.ResetPasswordAsync(identityUser, token, novaSenha);

            if (!result.Succeeded)
            {
                ViewBag.Erro = string.Join(" ", result.Errors.Select(e => e.Description));
                return View();
            }

            usuario.Senha = novaSenha;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Senha redefinida com sucesso. Faça login com sua nova senha.";
            return RedirectToAction("Login");
        }

        [HttpPost("ResetPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["Sucesso"] = "Senha redefinida com sucesso.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["Sucesso"] = "Senha redefinida com sucesso.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
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
        [HttpGet("RegisterCompany")]
        public IActionResult RegisterCompany() => View();

        [HttpPost("RegisterCompany")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCompany(Empresa model)
        {
            if (Request.Form["aceitaTermos"] != "on")
                ModelState.AddModelError("termos", "Você deve aceitar os Termos de Uso para continuar.");

            if (!ModelState.IsValid)
            {
                ViewBag.Erro = "Preencha todos os campos obrigatórios.";
                return View("RegisterCompany", model);
            }

            try
            {
                var identityUser = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(identityUser, model.Senha);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Empresa"))
                        await _roleManager.CreateAsync(new IdentityRole("Empresa"));

                    await _userManager.AddToRoleAsync(identityUser, "Empresa");

                    model.IdentityUserId = identityUser.Id;
                    model.DataCadastro = DateTime.Now;

                    _context.Empresas.Add(model);
                    await _context.SaveChangesAsync();

                    await _signInManager.SignInAsync(identityUser, isPersistent: false);
                    TempData["Sucesso"] = "Empresa cadastrada com sucesso!";
                    return RedirectToAction("Index", "Empresas");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View("RegisterCompany", model);
            }
            catch (Exception ex)
            {
                ViewBag.Erro = $"Erro ao salvar no banco: {ex.Message}";
                return View("RegisterCompany", model);
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
