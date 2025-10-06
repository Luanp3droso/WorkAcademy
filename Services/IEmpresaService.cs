using WorkAcademy.Models;

namespace WorkAcademy.Services
{
    public interface IEmpresaService
    {
        Task<bool> RegistrarEmpresa(Empresa empresa);
        Task<Empresa?> AutenticarEmpresa(string email, string senha);
        Task<Empresa?> ObterEmpresaPorId(Guid id);
        Task<IEnumerable<Empresa>> ObterTodasEmpresas();
        Task<bool> AtualizarEmpresa(Empresa empresa);
        Task<bool> RemoverEmpresa(Guid id);
    }
}