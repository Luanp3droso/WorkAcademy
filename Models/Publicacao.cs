using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkAcademy.Models
{
    public class Publicacao
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O conteúdo da publicação é obrigatório.")]
        [StringLength(500, ErrorMessage = "Máximo de 500 caracteres.")]
        public string Conteudo { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }
        public List<Comentario> Comentarios { get; set; }
        public List<PublicacaoCurtida> Curtidas { get; set; } = new();

    }
}
