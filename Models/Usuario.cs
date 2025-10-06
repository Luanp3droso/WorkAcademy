using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkAcademy.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome completo é obrigatório")]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; }

        [Required(ErrorMessage = "CPF é obrigatório")]
        [Display(Name = "CPF")]
        public string CPF { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; }

        [Required(ErrorMessage = "Endereço é obrigatório")]
        public string Endereco { get; set; }

        [Required(ErrorMessage = "Telefone é obrigatório")]
        public string Telefone { get; set; }

        [Required(ErrorMessage = "Celular é obrigatório")]
        public string Celular { get; set; }

        [Required(ErrorMessage = "Área de Interesse é obrigatória")]
        [Display(Name = "Área de Interesse")]
        public string AreaInteresse { get; set; }

        public string? IdentityUserId { get; set; }

        [ForeignKey("IdentityUserId")]
        public virtual Microsoft.AspNetCore.Identity.IdentityUser? IdentityUser { get; set; }

        public DateTime DataCadastro { get; set; }

        [Display(Name = "Foto de Perfil")]
        public string? FotoPerfil { get; set; }

        [Display(Name = "Currículo PDF")]
        public string? CurriculoPdf { get; set; }

        [Display(Name = "Biografia")]
        [StringLength(300)]
        public string? Biografia { get; set; }
    }
}
