using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAcademy.Models;

namespace WorkAcademy.Services.Notifications
{
    public interface INotificationService
    {
        Task<int> GetUnreadCountAsync(string userId);
        Task<IReadOnlyList<NotificationDto>> GetLatestAsync(string userId, int take = 10);
        Task MarkAllAsReadAsync(string userId);
        Task MarkAsReadAsync(string userId, int notificationId);

        Task<int> CreateAsync(NotificationCreate dto);

        Task<int> CreateFriendRequestAsync(string toUserId, string fromDisplayName, int? suggestedUserId = null);
        Task<int> CreateCourseNewAsync(string toUserId, string courseTitle, int courseId);
        Task<int> CreateJobNewAsync(string toUserId, string jobTitle, int vagaId);

        // (opcionais) eventos extra
        Task<int> CreateCertificateIssuedAsync(string toUserId, string courseTitle, int certificateId);
        Task<int> CreateMessageNewAsync(string toUserId, string fromDisplayName, int threadOrMessageId);
        Task<int> CreateJobApplicationAsync(string toUserId, string jobTitle, int candidaturaId);
    }
}
