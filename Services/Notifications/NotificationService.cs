using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkAcademy.Data;
using WorkAcademy.Models;
using WorkAcademy.Services.Notifications;

namespace WorkAcademy.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _db.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsArchived)
                .CountAsync();
        }

        public async Task<IReadOnlyList<NotificationDto>> GetLatestAsync(string userId, int take = 10)
        {
            var items = await _db.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsArchived)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Title = n.Title,
                    Message = n.Message,
                    Url = n.Url,
                    Icon = n.Icon,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return items;
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var ids = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .Select(n => n.Id)
                .ToListAsync();

            if (ids.Count == 0) return;

            var now = DateTime.UtcNow;
            await _db.Notifications
                .Where(n => ids.Contains(n.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, now));
        }

        public async Task MarkAsReadAsync(string userId, int notificationId)
        {
            await _db.Notifications
                .Where(n => n.Id == notificationId && n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, DateTime.UtcNow));
        }

        public async Task<int> CreateAsync(NotificationCreate dto)
        {
            var n = new Notification
            {
                UserId = dto.UserId,
                Type = dto.Type,
                Title = dto.Title,
                Message = dto.Message,
                Url = dto.Url,
                Icon = dto.Icon,
                ContentType = dto.ContentType,
                ContentId = dto.ContentId,
                CreatedAt = DateTime.UtcNow
            };
            _db.Notifications.Add(n);
            await _db.SaveChangesAsync();
            return n.Id;
        }

        public Task<int> CreateFriendRequestAsync(string toUserId, string fromDisplayName, int? suggestedUserId = null)
        {
            return CreateAsync(new NotificationCreate
            {
                UserId = toUserId,
                Type = "FriendRequest",
                Title = "Nova solicitação de amizade",
                Message = $"{fromDisplayName} enviou uma solicitação",
                Url = "/Usuario/Conexoes",
                Icon = "bi-person-plus",
                ContentType = "Usuario",
                ContentId = suggestedUserId
            });
        }

        public Task<int> CreateCourseNewAsync(string toUserId, string courseTitle, int courseId)
        {
            return CreateAsync(new NotificationCreate
            {
                UserId = toUserId,
                Type = "CourseNew",
                Title = $"Novo curso: {courseTitle}",
                Message = "Confira os detalhes",
                Url = $"/Cursos/Details/{courseId}",
                Icon = "bi-mortarboard",
                ContentType = "Curso",
                ContentId = courseId
            });
        }

        public Task<int> CreateJobNewAsync(string toUserId, string jobTitle, int vagaId)
        {
            return CreateAsync(new NotificationCreate
            {
                UserId = toUserId,
                Type = "JobNew",
                Title = $"Nova vaga: {jobTitle}",
                Message = "Veja os requisitos",
                Url = $"/Vagas/Details/{vagaId}",
                Icon = "bi-briefcase",
                ContentType = "Vaga",
                ContentId = vagaId
            });
        }

        // ---- extras (se você pediu) ----
        public Task<int> CreateCertificateIssuedAsync(string toUserId, string courseTitle, int certificateId)
        {
            return CreateAsync(new NotificationCreate
            {
                UserId = toUserId,
                Type = "CertificateIssued",
                Title = $"Certificado emitido: {courseTitle}",
                Message = "Seu certificado está disponível.",
                Url = $"/Certificados/Details/{certificateId}",
                Icon = "bi-award",
                ContentType = "Certificado",
                ContentId = certificateId
            });
        }

        public Task<int> CreateMessageNewAsync(string toUserId, string fromDisplayName, int threadOrMessageId)
        {
            return CreateAsync(new NotificationCreate
            {
                UserId = toUserId,
                Type = "MessageNew",
                Title = "Nova mensagem",
                Message = $"{fromDisplayName} enviou uma mensagem.",
                Url = $"/Mensagens/Thread/{threadOrMessageId}",
                Icon = "bi-chat-left-text",
                ContentType = "Mensagem",
                ContentId = threadOrMessageId
            });
        }

        public Task<int> CreateJobApplicationAsync(string toUserId, string jobTitle, int candidaturaId)
        {
            return CreateAsync(new NotificationCreate
            {
                UserId = toUserId,
                Type = "JobApplication",
                Title = $"Nova candidatura em: {jobTitle}",
                Message = "Você recebeu uma nova candidatura.",
                Url = $"/Candidaturas/Details/{candidaturaId}",
                Icon = "bi-file-earmark-text",
                ContentType = "Candidatura",
                ContentId = candidaturaId
            });
        }
    }
}
