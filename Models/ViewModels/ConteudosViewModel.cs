using System.Collections.Generic;
using WorkAcademy.Models;

namespace WorkAcademy.Models.ViewModels
{
    // ÚNICA definição!
    public class ConteudosViewModel
    {
        public List<Curso> Cursos { get; set; } = new();
        public List<Vaga> Vagas { get; set; } = new();
        public List<Publicacao> Publicacoes { get; set; } = new();

        // (opcional) contadores
        public int TotalCursos => Cursos?.Count ?? 0;
        public int TotalVagas => Vagas?.Count ?? 0;
        public int TotalPublicacoes => Publicacoes?.Count ?? 0;
    }
}
