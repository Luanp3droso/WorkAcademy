using WorkAcademy.Models;

namespace WorkAcademy.Services
{
    public interface IUsuarioService
    {
        Task<bool> RegistrarUsuario(Usuario usuario);
        Task<Usuario?> AutenticarUsuario(string email, string senha);
        Task<Usuario?> ObterUsuarioPorId(Guid id);
    }
}