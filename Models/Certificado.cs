using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkAcademy.Models
{
    public class Certificado
    {
        public int Id { get; set; }

        [Required]
        public string NomeArquivo { get; set; } = string.Empty;

        [Required]
        public string CaminhoArquivo { get; set; } = string.Empty;

        [Display(Name = "Data do Upload")]
        public DateTime DataUpload { get; set; } = DateTime.Now;

        [Display(Name = "Data de Conclusão")]
        [DataType(DataType.Date)]
        public DateTime DataConclusao { get; set; } = DateTime.Now;

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; } = new();

        [Required]
        public Guid CursoId { get; set; }

        [ForeignKey("CursoId")]
        public Curso Curso { get; set; } = new();
    }
}
