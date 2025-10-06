using Microsoft.EntityFrameworkCore;
using WorkAcademy.Data;
using WorkAcademy.Models;

namespace WorkAcademy.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;

        public UsuarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegistrarUsuario(Usuario usuario)
        {
            try
            {
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Usuario?> AutenticarUsuario(string email, string senha)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.Senha == senha);
        }

        public async Task<Usuario?> ObterUsuarioPorId(Guid id)
        {
            return await _context.Usuarios.FindAsync(id);
        }
    }
}
