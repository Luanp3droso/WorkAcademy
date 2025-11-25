using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkAcademy.Models
{
    public class Curso
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O nome do curso é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
        [Display(Name = "Nome do Curso")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(500, ErrorMessage = "A descrição pode ter no máximo 500 caracteres.")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        [Display(Name = "Categoria")]
        public string Categoria { get; set; }

        [Required(ErrorMessage = "O nível do curso é obrigatório.")]
        [Display(Name = "Nível")]
        public string Nivel { get; set; }

        [Required(ErrorMessage = "O valor do curso é obrigatório.")]
        [Range(0, 10000, ErrorMessage = "Informe um valor entre 0 e 10000.")]
        [Display(Name = "Valor")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "A duração é obrigatória.")]
        [Range(1, 36, ErrorMessage = "A duração deve ser entre 1 e 36 meses.")]
        [Display(Name = "Duração (meses)")]
        public int DuracaoMeses { get; set; }

        [Required(ErrorMessage = "A data de início é obrigatória.")]
        [Display(Name = "Data de Início")]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "A URL da imagem é obrigatória.")]
        [Url(ErrorMessage = "Informe uma URL válida.")]
        [Display(Name = "Imagem do Curso (URL)")]
        public string? ImagemUrl { get; set; }

        [Required]
        public Guid EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        public Empresa Empresa { get; set; }

        [Display(Name = "Autor do Curso (opcional)")]
        public string? Autor { get; set; }

        [Display(Name = "Link externo do curso (opcional)")]
        public string? Url { get; set; }

        [Display(Name = "Nome do Instrutor")]
        public string? NomeInstrutor { get; set; }

        [Range(0, 5)]
        [Display(Name = "Avaliação")]
        public double Avaliacao { get; set; }

        [Display(Name = "Total de Avaliações")]
        public int TotalAvaliacoes { get; set; }

        [Display(Name = "Preço")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoOriginal { get; set; }

        [Display(Name = "Mais Vendido")]
        public bool MaisVendido { get; set; } = false;

        public bool Ativo { get; set; } = true;
    }
}
