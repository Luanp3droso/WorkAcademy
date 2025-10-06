using System.ComponentModel.DataAnnotations;

namespace WorkAcademy.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais que 100 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Por favor, insira um email válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O assunto é obrigatório")]
        [StringLength(100, ErrorMessage = "O assunto não pode ter mais que 100 caracteres")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "A mensagem é obrigatória")]
        [StringLength(5000, ErrorMessage = "A mensagem não pode ter mais que 5000 caracteres")]
        public string Message { get; set; }
    }
}