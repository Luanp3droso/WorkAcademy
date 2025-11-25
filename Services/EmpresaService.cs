using Microsoft.EntityFrameworkCore;
using WorkAcademy.Data;
using WorkAcademy.Models;

namespace WorkAcademy.Services
{
    public class EmpresaService : IEmpresaService
    {
        private readonly ApplicationDbContext _context;

        public EmpresaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegistrarEmpresa(Empresa empresa)
        {
            try
            {
                _context.Empresas.Add(empresa);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Empresa?> AutenticarEmpresa(string email, string senha)
        {
            return await _context.Empresas
                .FirstOrDefaultAsync(e => e.Email == email && e.Senha == senha);
        }

        public async Task<Empresa?> ObterEmpresaPorId(Guid id)
        {
            return await _context.Empresas.FindAsync(id);
        }
        public async Task<Empresa?> ObterEmpresaPorUsuario(string identityUserId)
        {
            return await _context.Empresas
                .FirstOrDefaultAsync(e => e.IdentityUserId == identityUserId);
        }

        public async Task<IEnumerable<Empresa>> ObterTodasEmpresas()
        {
            return await _context.Empresas.ToListAsync();
        }

        public async Task<bool> AtualizarEmpresa(Empresa empresa)
        {
            try
            {
                _context.Empresas.Update(empresa);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoverEmpresa(Guid id)
        {
            try
            {
                var empresa = await _context.Empresas.FindAsync(id);
                if (empresa != null)
                {
                    _context.Empresas.Remove(empresa);
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