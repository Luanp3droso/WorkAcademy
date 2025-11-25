using System.ComponentModel.DataAnnotations;

namespace WorkAcademy.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}