using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using System.Threading.Tasks;

namespace Azure.Functions.Identity.Web
{
    public class SampleFunc
    {
        private readonly ILogger<SampleFunc> logger;

        // The web API will only accept tokens 1) for users, and 2) having the "access_as_user" scope for this API
        private static readonly string[] scopeRequiredByApi = new string[] { "access_as_user" };

        public SampleFunc(ILogger<SampleFunc> logger)
        {
            this.logger = logger;
        }

        [FunctionName("SampleFunc")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            logger.LogInformation("Executing Authentication Request...");

            var (authenticationStatus, authenticationResponse) =
                await req.HttpContext.AuthenticateAzureFunctionAsync();

            logger.LogInformation($"Authentication Success: {authenticationStatus}.");

            if (!authenticationStatus) return authenticationResponse;

            req.HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

            string name = req.HttpContext.User.Identity.IsAuthenticated ? req.HttpContext.User.GetDisplayName() : null;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "Could not authenticate user"
                : $"User {name} authenticated successfully.";

            return new JsonResult(responseMessage);
        }
    }
}