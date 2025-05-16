using System.Threading.Tasks;

namespace TokenManager.Service
{
    public interface ITokenManagerService
    {
        Task AddORUpdateFNOToken();
        Task AddORUpdateCEToken();

    }
}
