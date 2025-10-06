using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkAcademy.Models
{
    public class Comentario
    {
        public int Id { get; set; }

        [Required]
        public string Conteudo { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Required]
        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; }

        [Required]
        public int PublicacaoId { get; set; }

        public Publicacao Publicacao { get; set; }

        public List<CurtidaComentario> Curtidas { get; set; } = new();
    }
}