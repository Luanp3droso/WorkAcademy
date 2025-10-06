using System;
using System.ComponentModel.DataAnnotations;

namespace WorkAcademy.Models
{
    public class Denuncia
    {
        public int Id { get; set; }

        [Required]
        public string Tipo { get; set; } // Ex: Curso, Vaga

        [Required]
        public int ConteudoId { get; set; } // ID do curso, vaga etc.

        [Required]
        public string Motivo { get; set; }

        public string UsuarioId { get; set; }

        public DateTime Data { get; set; } = DateTime.Now;

        public bool Resolvido { get; set; } = false;
    }
}
