using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WorkAcademy.Data;
using WorkAcademy.Models;
using Microsoft.EntityFrameworkCore;

namespace WorkAcademy.Controllers
{
    [Authorize]
    [Route("Usuario")]
    public class UsuarioController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsuarioController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("Perfil")]
        public async Task<IActionResult> Perfil()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return RedirectToAction("Login", "Account");

            // Certificados
            var certificados = await _context.Certificados
                .Include(c => c.Curso)
                .Where(c => c.UsuarioId == usuario.Id)
                .ToListAsync();

            // Publicações
            var publicacoes = await _context.Publicacoes
                .Include(p => p.Usuario)
                .Where(p => p.UsuarioId == usuario.Id)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();

            // Conexões aceitas com includes para evitar erro
            var conexoes = await _context.Conexoes
                .Include(c => c.Usuario)
                .Include(c => c.ConectadoCom)
                .Where(c =>
                    (c.UsuarioId == usuario.Id || c.ConectadoComId == usuario.Id) &&
                    c.Status == "Aceita")
                .ToListAsync();

            // Enviar todos os dados necessários para a view
            ViewBag.Nome = usuario.NomeCompleto;
            ViewBag.Email = usuario.Email;
            ViewBag.CPF = usuario.CPF;
            ViewBag.Endereco = usuario.Endereco;
            ViewBag.Telefone = usuario.Telefone;
            ViewBag.Celular = usuario.Celular;
            ViewBag.AreaInteresse = usuario.AreaInteresse;
            ViewBag.DataCadastro = usuario.DataCadastro.ToString("dd/MM/yyyy");
            ViewBag.FotoPerfil = usuario.FotoPerfil;
            ViewBag.CurriculoPdf = usuario.CurriculoPdf;
            ViewBag.Biografia = usuario.Biografia;
            ViewBag.Conexoes = conexoes;
            ViewBag.Certificados = certificados;
            ViewBag.Publicacoes = publicacoes;

            return View(usuario);
        }

        [HttpGet("Home")]
        public async Task<IActionResult> Home()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var publicacoes = await _context.Publicacoes
                .Include(p => p.Usuario)
                .Include(p => p.Comentarios).ThenInclude(c => c.Usuario)
                .Include(p => p.Curtidas)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();

            var conexoesAceitas = await _context.Conexoes
                .Where(c =>
                    (c.UsuarioId == usuario.Id || c.ConectadoComId == usuario.Id) &&
                    c.Status == "Aceita")
                .ToListAsync();

            var conexoesIds = conexoesAceitas
                .Select(c => c.UsuarioId == usuario.Id ? c.ConectadoComId : c.UsuarioId)
                .Distinct()
                .ToList();

            // Corrigido: pegar pendentes *enviados e recebidos*
            var conexoesPendentes = await _context.Conexoes
                .Where(c =>
                    (c.UsuarioId == usuario.Id || c.ConectadoComId == usuario.Id) &&
                    c.Status == "Pendente")
                .ToListAsync();

            var idsPendentes = conexoesPendentes
                .Select(c => c.UsuarioId == usuario.Id ? c.ConectadoComId : c.UsuarioId)
                .Distinct()
                .ToList();

            // Sugestões = todos os usuários que não estão conectados nem pendentes
            var outrosUsuarios = await _context.Usuarios
                .Where(u =>
                    u.Id != usuario.Id &&
                    !conexoesIds.Contains(u.Id) &&
                    !idsPendentes.Contains(u.Id))
                .ToListAsync();

            var pendentes = await _context.Conexoes
                .Include(c => c.Usuario)
                .Where(c => c.ConectadoComId == usuario.Id && c.Status == "Pendente")
                .ToListAsync();

            var amigos = await _context.Usuarios
                .Where(u => conexoesIds.Contains(u.Id))
                .ToListAsync();

            ViewBag.UsuarioId = usuario.Id;
            ViewBag.Nome = usuario.NomeCompleto;
            ViewBag.Publicacoes = publicacoes
                .Where(p => conexoesIds.Contains(p.UsuarioId) || p.UsuarioId == usuario.Id)
                .ToList();
            ViewBag.OutrosUsuarios = outrosUsuarios;
            ViewBag.Pendentes = pendentes;
            ViewBag.Amigos = amigos;

            return View();
        }

        [HttpPost("CurtirComentario")]
        public async Task<IActionResult> CurtirComentario(int comentarioId, int usuarioId)
        {
            var jaCurtiu = await _context.CurtidasComentario
                .FirstOrDefaultAsync(c => c.ComentarioId == comentarioId && c.UsuarioId == usuarioId);

            if (jaCurtiu == null)
            {
                var curtida = new CurtidaComentario
                {
                    ComentarioId = comentarioId,
                    UsuarioId = usuarioId
                };
                _context.CurtidasComentario.Add(curtida);
            }
            else
            {
                _context.CurtidasComentario.Remove(jaCurtiu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Home");
        }

        [HttpPost("CurtirPublicacao")]
        public async Task<IActionResult> CurtirPublicacao(int publicacaoId, int usuarioId)
        {
            var jaCurtiu = await _context.PublicacaoCurtidas
                .FirstOrDefaultAsync(c => c.PublicacaoId == publicacaoId && c.UsuarioId == usuarioId);

            if (jaCurtiu == null)
            {
                _context.PublicacaoCurtidas.Add(new PublicacaoCurtida
                {
                    PublicacaoId = publicacaoId,
                    UsuarioId = usuarioId
                });
            }
            else
            {
                _context.PublicacaoCurtidas.Remove(jaCurtiu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Home");
        }

        [HttpGet("EditarComentario/{id}")]
        public async Task<IActionResult> EditarComentario(int id)
        {
            var comentario = await _context.Comentarios.FirstOrDefaultAsync(c => c.Id == id);
            if (comentario == null || comentario.UsuarioId != GetUsuarioId())
                return RedirectToAction("Home");
            return View(comentario);
        }

        [HttpPost("EditarComentario")]
        public async Task<IActionResult> EditarComentario(Comentario model)
        {
            if (!ModelState.IsValid) return View(model);

            var comentario = await _context.Comentarios.FindAsync(model.Id);
            if (comentario == null || comentario.UsuarioId != GetUsuarioId())
                return RedirectToAction("Home");

            comentario.Conteudo = model.Conteudo;
            await _context.SaveChangesAsync();
            return RedirectToAction("Home");
        }

        [HttpPost("ExcluirComentario")]
        public async Task<IActionResult> ExcluirComentario(int comentarioId)
        {
            var comentario = await _context.Comentarios.FindAsync(comentarioId);
            if (comentario != null && comentario.UsuarioId == GetUsuarioId())
            {
                _context.Comentarios.Remove(comentario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Home");
        }

        private int GetUsuarioId()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            return _context.Usuarios.FirstOrDefault(u => u.Email == email)?.Id ?? 0;
        }


        [HttpPost("Publicar")]
        public async Task<IActionResult> Publicar(string conteudo, int usuarioId)
        {
            if (!string.IsNullOrWhiteSpace(conteudo))
            {
                var publicacao = new Publicacao
                {
                    Conteudo = conteudo,
                    UsuarioId = usuarioId,
                    DataCriacao = DateTime.Now
                };

                _context.Publicacoes.Add(publicacao);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Home");
        }

        [HttpPost("Conectar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Conectar(int usuarioId, int conectadoComId)
        {
            if (usuarioId == conectadoComId)
                return RedirectToAction("Home");

            var jaExiste = await _context.Conexoes.AnyAsync(c =>
                (c.UsuarioId == usuarioId && c.ConectadoComId == conectadoComId) ||
                (c.UsuarioId == conectadoComId && c.ConectadoComId == usuarioId));

            if (!jaExiste)
            {
                var conexao = new Conexao
                {
                    UsuarioId = usuarioId,
                    ConectadoComId = conectadoComId,
                    Status = "Pendente"
                };

                _context.Conexoes.Add(conexao);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Home");
        }


        [HttpPost("Aceitar")]
        public async Task<IActionResult> Aceitar(int idConexao)
        {
            var conexao = await _context.Conexoes.FindAsync(idConexao);
            if (conexao != null)
            {
                conexao.Status = "Aceita";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Home");
        }

        [HttpPost("Desconectar")]
        public async Task<IActionResult> Desconectar(int usuarioId, int conectadoComId)
        {
            var conexao = await _context.Conexoes
                .FirstOrDefaultAsync(c =>
                    (c.UsuarioId == usuarioId && c.ConectadoComId == conectadoComId) ||
                    (c.UsuarioId == conectadoComId && c.ConectadoComId == usuarioId));

            if (conexao != null)
            {
                _context.Conexoes.Remove(conexao);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Perfil");
        }

        [HttpGet("Ver/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Ver(int id)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return NotFound();

            var publicacoes = await _context.Publicacoes
                .Include(p => p.Usuario)
                .Where(p => p.UsuarioId == id)
                .ToListAsync();

            var conexoes = await _context.Conexoes
                .Include(c => c.Usuario)
                .Include(c => c.ConectadoCom)
                .Where(c => (c.UsuarioId == id || c.ConectadoComId == id) && c.Status == "Aceita")
                .ToListAsync();

            var certificados = await _context.Certificados
                .Include(c => c.Curso)
                .Where(c => c.UsuarioId == id)
                .ToListAsync();

            ViewBag.Publicacoes = publicacoes;
            ViewBag.Conexoes = conexoes;
            ViewBag.Certificados = certificados;

            return View("Perfil", usuario);
        }

        [HttpPost("AtualizarFoto")]
        public async Task<IActionResult> AtualizarFoto(IFormFile fotoPerfil)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return RedirectToAction("Login", "Account");

            if (fotoPerfil != null && fotoPerfil.Length > 0)
            {
                var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var pastaFotos = Path.Combine(wwwRoot, "img", "usuarios");
                if (!Directory.Exists(pastaFotos))
                    Directory.CreateDirectory(pastaFotos);

                var nomeArquivo = $"foto_{usuario.Id}_{DateTime.Now.Ticks}{Path.GetExtension(fotoPerfil.FileName)}";
                var caminhoCompleto = Path.Combine(pastaFotos, nomeArquivo);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await fotoPerfil.CopyToAsync(stream);
                }

                usuario.FotoPerfil = $"/img/usuarios/{nomeArquivo}";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Perfil");
        }

        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdentityUserId == userId);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(Usuario model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdentityUserId == userId);

            if (usuario == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                usuario.NomeCompleto = model.NomeCompleto;
                usuario.Senha = model.Senha;

                _context.Update(usuario);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Perfil atualizado com sucesso!";
                return RedirectToAction("Perfil");
            }

            TempData["Erro"] = "Erro ao atualizar perfil.";
            return View(model);
        }

        [HttpPost("Comentar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comentar(int publicacaoId, int usuarioId, string conteudo)
        {
            if (string.IsNullOrWhiteSpace(conteudo))
            {
                TempData["Erro"] = "Comentário não pode estar vazio.";
                return RedirectToAction("Home");
            }

            var comentario = new Comentario
            {
                PublicacaoId = publicacaoId,
                UsuarioId = usuarioId,
                Conteudo = conteudo,
            };

            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync();

            return RedirectToAction("Home");
        }

        [HttpGet("Conexoes")]
        public async Task<IActionResult> Conexoes()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            // ✅ Todas as conexões aceitas, em qualquer direção
            var conexoes = await _context.Conexoes
                .Include(c => c.Usuario)
                .Include(c => c.ConectadoCom)
                .Where(c =>
                    (c.UsuarioId == usuario.Id || c.ConectadoComId == usuario.Id) &&
                    c.Status == "Aceita")
                .ToListAsync();

            // ✅ Determina o "amigo" da conexão (não importa o lado)
            var amigos = conexoes
                .Select(c => c.UsuarioId == usuario.Id ? c.ConectadoCom : c.Usuario)
                .ToList();

            ViewBag.UsuarioId = usuario.Id;
            ViewBag.Amigos = amigos;
            ViewBag.Nome = usuario.NomeCompleto;

            return View();
        }

        [HttpPost("ExcluirPublicacao")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirPublicacao(int publicacaoId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var publicacao = await _context.Publicacoes
                .FirstOrDefaultAsync(p => p.Id == publicacaoId && p.UsuarioId == usuario.Id);

            if (publicacao == null)
            {
                TempData["Erro"] = "Publicação não encontrada ou acesso negado.";
                return RedirectToAction("Home");
            }

            _context.Publicacoes.Remove(publicacao);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Publicação excluída com sucesso.";
            return RedirectToAction("Home");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

    }
}
