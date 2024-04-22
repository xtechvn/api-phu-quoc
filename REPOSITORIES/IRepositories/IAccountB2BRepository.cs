using ENTITIES.Models;
using ENTITIES.ViewModels.Client;
using System.Threading.Tasks;

namespace REPOSITORIES.IRepositories
{
    public interface IAccountB2BRepository
    {
        Task<AccountClient> GetAccountClientById(long accountClientId);
        Task<ClientB2BDetailUpdateViewModel> GetClientB2BDetailViewModel(long clientId);
        Task<long> UpdateClientDetail(ClientB2BDetailUpdateViewModel model, long clientId);
    }
}
