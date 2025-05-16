using System.Threading.Tasks;

namespace TokenManager.Service
{
    public interface IAuthService
    {
        Task<string> GenerateToken(string instance, string tenant, string cliendID, string ClientSecret, string baseUrl);
    }
}
