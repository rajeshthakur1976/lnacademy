using Microsoft.Identity.Client;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace TokenManager.Service
{
    public class AuthService : IAuthService
    {

        /// <summary>
        /// Generate AD Token
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="tenant"></param>
        /// <param name="cliendID"></param>
        /// <param name="ClientSecret"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public async Task<string> GenerateToken(string instance, string tenant, string cliendID, string ClientSecret, string baseUrl)
        {
            try
            {
                string Authority = String.Format(CultureInfo.InvariantCulture, instance, tenant);
                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(cliendID)
                        .WithClientSecret(ClientSecret)
                        .WithAuthority(new Uri(Authority))
                        .Build();
                string[] scopes = new string[] { $"{baseUrl}/.default" };
                AuthenticationResult result = await app.AcquireTokenForClient(scopes)
                    .ExecuteAsync();
                return result.CreateAuthorizationHeader();
            }
            catch (Exception ex)
            {
                throw ;
            }
        }
    }
    
}
