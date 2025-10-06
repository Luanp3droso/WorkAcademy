using System;

namespace WorkAcademy.Models
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Message { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
