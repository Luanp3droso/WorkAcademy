using System;
using System.ComponentModel.DataAnnotations;

namespace WorkAcademy.Models
{
    public class Vaga
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Título é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Descrição é obrigatória")]
        [DataType(DataType.MultilineText)]
        public string Descricao { get; set; }

        public Guid? EmpresaId { get; set; } // Tornado nullable para evitar erro de binding
        public Empresa? Empresa { get; set; }

        [Required(ErrorMessage = "Área é obrigatória")]
        public string Area { get; set; }

        [Display(Name = "Salário")]
        [DataType(DataType.Currency)]
        public decimal? Salario { get; set; }

        [Required(ErrorMessage = "Localização é obrigatória")]
        public string Localizacao { get; set; }

        [Required(ErrorMessage = "Tipo de contrato é obrigatório")]
        [Display(Name = "Tipo de Contrato")]
        public string TipoContrato { get; set; }

        [Display(Name = "Data de Publicação")]
        public DateTime DataPublicacao { get; set; } = DateTime.Now;

        [Display(Name = "Data de Expiração")]
        public DateTime? DataExpiracao { get; set; }

        [Display(Name = "Aprovada pelo Administrador")]
        public bool Aprovada { get; set; } = false;

        [Display(Name = "Motivo da Rejeição")]
        [StringLength(1000)]
        public string? MotivoRejeicao { get; set; }

    }
}
