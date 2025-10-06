using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkAcademy.Models
{
    public class CursoUsuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Evita que o EF interprete como IDENTITY
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CursoId { get; set; }

        [ForeignKey("CursoId")]
        public Curso Curso { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        public string UsuarioIdentityId { get; set; }

        public DateTime DataInscricao { get; set; } = DateTime.Now;

        public bool Concluido { get; set; } = false;
    }
}
