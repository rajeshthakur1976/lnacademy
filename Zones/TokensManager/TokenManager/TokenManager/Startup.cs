using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(TokenManager.Startup))]
namespace TokenManager
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<DemoService>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            //var config= builder.ConfigurationBuilder.Build();
            //config.AddAzureKeyVault();
            //// local.settings.json are automatically loaded when debugging.
            //// When running on Azure, values are loaded defined in app settings. See: https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings
            builder.ConfigurationBuilder
                .AddAppSettingsJson(builder.GetContext())
                .AddEnvironmentVariables()
                //.AddUserSecrets<Startup>(true)
                .AddAzureKeyVault()

                .Build();
        }
    }
}
