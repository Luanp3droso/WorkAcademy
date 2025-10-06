using Microsoft.AspNetCore.Mvc;
using WorkAcademy.Models;
using System.Net.Mail;
using System.Net;

namespace WorkAcademy.Controllers
{
    public class ContactController : Controller
    {
        private readonly IConfiguration _config;

        public ContactController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Configuração do e-mail
                    var smtpClient = new SmtpClient(_config["EmailSettings:SmtpServer"])
                    {
                        Port = int.Parse(_config["EmailSettings:Port"]),
                        Credentials = new NetworkCredential(
                            _config["EmailSettings:Username"],
                            _config["EmailSettings:Password"]),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(model.Email),
                        Subject = $"Contato WorkAcademy - {model.Subject}",
                        Body = $"Nome: {model.Name}\nEmail: {model.Email}\n\nMensagem:\n{model.Message}",
                        IsBodyHtml = false,
                    };

                    mailMessage.To.Add(_config["EmailSettings:ToAddress"]);

                    smtpClient.Send(mailMessage);

                    TempData["SuccessMessage"] = "Mensagem enviada com sucesso! Entraremos em contato em breve.";
                    return RedirectToAction("Index");
                }
                catch
                {
                    ModelState.AddModelError("", "Ocorreu um erro ao enviar sua mensagem. Por favor, tente novamente mais tarde.");
                }
            }

            return View(model);
        }
    }
}