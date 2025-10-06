using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkAcademy.Models
{
    public class Conexao
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int ConectadoComId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pendente"; // <-- Adicionado

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [ForeignKey("ConectadoComId")]
        public Usuario ConectadoCom { get; set; }
    }
}
