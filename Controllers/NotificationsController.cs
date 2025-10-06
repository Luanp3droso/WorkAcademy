using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkAcademy.Services.Notifications;

namespace WorkAcademy.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet("Count")]
        public async Task<IActionResult> Count()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(); // evita passar null e elimina aviso CS8604

            var count = await _service.GetUnreadCountAsync(userId);
            return Json(new { count });
        }

        [HttpGet("Latest")]
        public async Task<IActionResult> Latest(int take = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var items = await _service.GetLatestAsync(userId, take);
            return Json(items);
        }

        // Pode trocar para POST se preferir; mantive GET no MVP
        [HttpGet("MarkAllRead")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            await _service.MarkAllAsReadAsync(userId);
            return Ok();
        }

        [HttpGet("MarkRead/{id:int}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            await _service.MarkAsReadAsync(userId, id);
            return Ok();
        }
    }
}
