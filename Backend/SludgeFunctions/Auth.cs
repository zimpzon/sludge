using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Sludge
{
    public static class Auth
    {
        [FunctionName("gettoken")]
        public static async Task<IActionResult> GetToken([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            string userId = req.Query["userId"];
            await Task.CompletedTask;
            return new OkObjectResult(userId);
        }
    }
}
