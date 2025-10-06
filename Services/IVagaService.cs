using WorkAcademy.Models;

namespace WorkAcademy.Services
{
    public interface IVagaService
    {
        Task<IEnumerable<Vaga>> ObterTodasVagas();
        Task<Vaga?> ObterVagaPorId(Guid id);
        Task<bool> AdicionarVaga(Vaga vaga);
        Task<bool> AtualizarVaga(Vaga vaga);
        Task<bool> RemoverVaga(Guid id);
    }
}