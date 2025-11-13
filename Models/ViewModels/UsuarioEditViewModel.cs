using System.ComponentModel.DataAnnotations;

namespace WorkAcademy.Models.ViewModels
{
    public class UsuarioEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nome Completo")]
        [Required(ErrorMessage = "Informe o nome completo.")]
        [StringLength(100)]
        public string? NomeCompleto { get; set; }

        [Display(Name = "Biografia")]
        [StringLength(1000)]
        public string? Biografia { get; set; }

        [Display(Name = "Endereço")]
        [StringLength(200)]
        public string? Endereco { get; set; }

        [Display(Name = "Celular")]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Informe um celular com 10 ou 11 dígitos (somente números).")]
        public string? Celular { get; set; }

        [Display(Name = "Área de Interesse")]
        [StringLength(100)]
        public string? AreaInteresse { get; set; }
    }
}
