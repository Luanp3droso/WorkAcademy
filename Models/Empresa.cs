using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkAcademy.Models
{
    public class Empresa
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Display(Name = "Razão Social")]
        public string? RazaoSocial { get; set; }

        [Display(Name = "Nome Fantasia")]
        public string? NomeFantasia { get; set; }

        [StringLength(18, ErrorMessage = "CNPJ deve ter 14 dígitos")]
        public string? CNPJ { get; set; }

        [Display(Name = "Inscrição Estadual")]
        public string? InscricaoEstadual { get; set; }

        [Display(Name = "Ramo de Atividade")]
        public string? RamoAtividade { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? Email { get; set; }

        [DataType(DataType.Password)]
        public string? Senha { get; set; }

        public string? Endereco { get; set; }

        [Phone(ErrorMessage = "Telefone inválido")]
        public string? Telefone { get; set; }

        [Phone(ErrorMessage = "Celular inválido")]
        public string? Celular { get; set; }

        [Display(Name = "Sobre a Empresa")]
        public string? SobreEmpresa { get; set; }

        [Display(Name = "Vagas Disponíveis")]
        public int? VagasDisponiveis { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public virtual ICollection<Vaga> Vagas { get; set; } = new List<Vaga>();

        // AGORA É OPCIONAL
        public string? IdentityUserId { get; set; }
    }
}
