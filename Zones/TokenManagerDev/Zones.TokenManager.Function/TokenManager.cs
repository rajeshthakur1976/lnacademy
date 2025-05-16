using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TokenManager.Service;

namespace TokenManager
{
    public class TokenManager
    {
        private readonly ITokenManagerService _tokenManagerService = null;

        public TokenManager(ITokenManagerService tokenManagerService)
        {
            _tokenManagerService = tokenManagerService;

        }
        [FunctionName("PrimingCache")]
        public async Task Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Token Generation Process Started at {DateTime.Now}");

            //await _tokenManagerService.AddORUpdateFNOToken();
            //await _tokenManagerService.AddORUpdateCEToken();

            log.LogInformation($"Token Generation Process Ended at {DateTime.Now}");
        }



    }
}
