using Microsoft.Identity.Web;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Azure.Functions.Identity.Web.Extensions;

[assembly: FunctionsStartup(typeof(Azure.Functions.Identity.Web.Startup))]

namespace Azure.Functions.Identity.Web
{
    public class Startup : FunctionsStartup
    {
        public Startup()
        {
        }

        private IConfiguration Configuration { get; set; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Get the azure function application directory. 'C:\whatever' for local and 'd:\home\whatever' for Azure
            var executionContextOptions = builder.Services.BuildServiceProvider()
                .GetService<IOptions<ExecutionContextOptions>>().Value;

            var currentDirectory = executionContextOptions.AppDirectory;

            // Get the original configuration provider from the Azure Function
            var configuration = builder.Services.BuildServiceProvider().GetService<IConfiguration>();

            // Create a new IConfigurationRoot and add our configuration along with Azure's original configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddConfiguration(configuration) // Add the original function configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Replace the Azure Function configuration with our new one
            builder.Services.AddSingleton(Configuration);

            ConfigureServices(builder.Services);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = Constants.Bearer;
                    sharedOptions.DefaultChallengeScheme = Constants.Bearer;
                })
                .AddArmToken()
                .AddScriptAuthLevel()
                .AddMicrosoftIdentityWebApi(Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();

            services
                .AddAuthorization(options => options.AddScriptPolicies());

            services
                .AddAuthLevelAuthorizationHandler()
                .AddNamedAuthLevelAuthorizationHandler()
                .AddFunctionAuthorizationHandler();
        }
    }
}