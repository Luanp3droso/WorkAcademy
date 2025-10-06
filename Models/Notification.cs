using System;
using System.ComponentModel.DataAnnotations;

namespace WorkAcademy.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(450)]
        public string UserId { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Type { get; set; } = null!; // CourseNew, JobNew, FriendRequest...

        [Required, MaxLength(120)]
        public string Title { get; set; } = null!;

        [MaxLength(260)]
        public string? Message { get; set; }

        [MaxLength(260)]
        public string? Url { get; set; }

        [MaxLength(40)]
        public string? Icon { get; set; } // ex.: bi-briefcase

        [MaxLength(60)]
        public string? ContentType { get; set; } // "Curso", "Vaga", "Usuario"

        public int? ContentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public bool IsRead { get; set; } = false;
        public bool IsArchived { get; set; } = false;
    }
}
