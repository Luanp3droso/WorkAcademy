using Microsoft.EntityFrameworkCore;
using WorkAcademy.Data;
using WorkAcademy.Models;

namespace WorkAcademy.Services
{
    public class VagaService : IVagaService
    {
        private readonly ApplicationDbContext _context;

        public VagaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vaga>> ObterTodasVagas()
        {
            return await _context.Vagas
                .Include(v => v.Empresa)
                .ToListAsync();
        }

        public async Task<Vaga?> ObterVagaPorId(Guid id)
        {
            return await _context.Vagas
                .Include(v => v.Empresa)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<bool> AdicionarVaga(Vaga vaga)
        {
            try
            {
                _context.Vagas.Add(vaga);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AtualizarVaga(Vaga vaga)
        {
            try
            {
                _context.Vagas.Update(vaga);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoverVaga(Guid id)
        {
            try
            {
                var vaga = await _context.Vagas.FindAsync(id);
                if (vaga != null)
                {
                    _context.Vagas.Remove(vaga);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}