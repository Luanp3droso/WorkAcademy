using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAcademy.Models;

namespace WorkAcademy.Services
{
    public interface ICursoService
    {
        Task<IEnumerable<Curso>> ObterTodosCursos();
        Task<Curso> ObterCursoPorId(Guid id);
        Task AdicionarCurso(Curso curso);
        Task AtualizarCurso(Curso curso);
        Task RemoverCurso(Guid id);
    }
}