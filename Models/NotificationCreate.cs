namespace WorkAcademy.Models
{
    public class NotificationCreate
    {
        public string UserId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Message { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public string? ContentType { get; set; }
        public int? ContentId { get; set; }
    }
}
