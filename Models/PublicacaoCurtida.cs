using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkAcademy.Models
{
    public class PublicacaoCurtida
    {
        public int Id { get; set; }

        [Required]
        public int PublicacaoId { get; set; }

        [ForeignKey(nameof(PublicacaoId))]
        public Publicacao Publicacao { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; }
    }
}
