using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkAcademy.Models
{
    public class CurtidaComentario
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; }

        [Required]
        public int ComentarioId { get; set; }

        [ForeignKey(nameof(ComentarioId))]
        public Comentario Comentario { get; set; }
    }
}
