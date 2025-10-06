using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkAcademy.Data;
using WorkAcademy.Models;
using WorkAcademy.Services.Notifications;

namespace WorkAcademy.Controllers
{
    [Authorize]
    public class CertificadosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CertificadosController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            ViewBag.Cursos = new SelectList(_context.Cursos.ToList(), "Id", "Nome");
            ViewBag.Usuarios = new SelectList(_context.Usuarios.ToList(), "Id", "NomeCompleto");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile arquivo, int usuarioId, Guid cursoId)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                ModelState.AddModelError("arquivo", "Selecione um arquivo válido.");
                ViewBag.Cursos = new SelectList(_context.Cursos.ToList(), "Id", "Nome");
                ViewBag.Usuarios = new SelectList(_context.Usuarios.ToList(), "Id", "NomeCompleto");
                return View();
            }

            var uploadsPath = Path.Combine(_environment.WebRootPath, "certificados");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = Path.GetFileName(arquivo.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            var certificado = new Certificado
            {
                NomeArquivo = fileName,
                CaminhoArquivo = "/certificados/" + fileName,
                UsuarioId = usuarioId,
                CursoId = cursoId,
                DataUpload = DateTime.Now
            };

            _context.Certificados.Add(certificado);
            await _context.SaveChangesAsync();

            TempData["Mensagem"] = "Certificado enviado com sucesso!";
            return RedirectToAction("Upload");
        }
    }
}
