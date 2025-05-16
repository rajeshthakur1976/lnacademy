using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using TokenManager.Cache;
using TokenManager.Service;

[assembly: FunctionsStartup(typeof(TokenManager.Startup))]
namespace TokenManager
{
    class Startup : FunctionsStartup
    {
        private IConfiguration _configuration = null;
        private ILoggerFactory _loggerFactory;
        /// <summary>
        /// Configure App Configuration
        /// </summary>
        /// <param name="builder"></param>
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(
                        azureServiceTokenProvider.KeyVaultTokenCallback));

            _configuration = builder.ConfigurationBuilder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                 .AddAzureKeyVault(Environment.GetEnvironmentVariable("AzureKeyVaultEndPoint"),keyVaultClient,new DefaultKeyVaultSecretManager())
                 .Build();

        }

        public override void Configure(IFunctionsHostBuilder builder)
        {

            builder.Services.AddLogging();
            builder.Services.AddSingleton<ICacheProvider, AzureCacheProvider>();
            builder.Services.AddSingleton<IConfiguration>(_configuration);
            builder.Services.AddSingleton<ITokenManagerService, TokenManagerService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();

        }

    }
}
