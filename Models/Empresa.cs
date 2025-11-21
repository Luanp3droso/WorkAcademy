using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkAcademy.Models
{
    public class Empresa
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Razão social é obrigatória")]
        [Display(Name = "Razão Social")]
        public string RazaoSocial { get; set; }

        [Required(ErrorMessage = "Nome fantasia é obrigatório")]
        [Display(Name = "Nome Fantasia")]
        public string NomeFantasia { get; set; }

        [Required(ErrorMessage = "CNPJ é obrigatório")]
        [StringLength(18, ErrorMessage = "CNPJ deve ter 14 dígitos")]
        public string CNPJ { get; set; }

        [Display(Name = "Inscrição Estadual")]
        [Required(ErrorMessage = "Inscrição Estadual é obrigatória")]
        public string InscricaoEstadual { get; set; }

        [Required(ErrorMessage = "Ramo de atividade é obrigatório")]
        [Display(Name = "Ramo de Atividade")]
        public string RamoAtividade { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        [DataType(DataType.Password)]
        public string Senha { get; set; }

        [Required(ErrorMessage = "Endereço é obrigatório")]
        public string Endereco { get; set; }

        [Required(ErrorMessage = "Telefone é obrigatório")]
        [Phone(ErrorMessage = "Telefone inválido")]
        public string Telefone { get; set; }

        [Required(ErrorMessage = "Celular é obrigatório")]
        [Phone(ErrorMessage = "Celular inválido")]
        public string Celular { get; set; }

        [Display(Name = "Sobre a Empresa")]
        public string? SobreEmpresa { get; set; }

        [Display(Name = "Vagas Disponíveis")]
        public int? VagasDisponiveis { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public virtual ICollection<Vaga> Vagas { get; set; } = new List<Vaga>();

        [Required(ErrorMessage = "Usuário vinculado (Identity) é obrigatório")]
        public string IdentityUserId { get; set; }
    }
}