using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAcademy.Models;
using WorkAcademy.Data;
using Microsoft.EntityFrameworkCore;

namespace WorkAcademy.Services
{
    public class CursoService : ICursoService
    {
        private readonly ApplicationDbContext _context;

        public CursoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarCurso(Curso curso)
        {
            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarCurso(Curso curso)
        {
            _context.Update(curso);
            await _context.SaveChangesAsync();
        }

        public async Task<Curso> ObterCursoPorId(Guid id)
        {
            return await _context.Cursos.FindAsync(id);
        }

        public async Task<IEnumerable<Curso>> ObterTodosCursos()
        {
            return await _context.Cursos.ToListAsync();
        }

        public async Task RemoverCurso(Guid id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();
        }
    }
}